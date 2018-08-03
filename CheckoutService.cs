using System;
using System.Collections.Generic;
using System.Linq;
using LibraryExercise.Models;
using LibraryExercise.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryExercise
{
    public class CheckoutService : ICheckoutService
    {
        private readonly LibraryContext _context;

        public CheckoutService(LibraryContext context)
        {
            this._context = context;
        }
        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll().FirstOrDefault(checkout => checkout.Id == checkoutId);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == id);
        }

        public IEnumerable<AssetHoldModel> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Where(h => h.LibraryAsset.Id == id); 
        }

        public Checkout GetLatestCheckout(int assetId)
        {
            return _context.Checkouts
                .Where(c => c.LibraryAsset.Id == assetId)
                .OrderByDescending(c => c.Since)
                .FirstOrDefault();        
        }
        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;

            UpdateAssetStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            ClosingExistingCheckoutHistory(assetId, now);

            _context.SaveChanges();
        }

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");
            _context.SaveChanges();
        }

        public void CheckinItem(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            // remove any exisiting checkouts on the item
            RemoveExistingCheckouts(assetId);

            // close any exisiting checkouts history
            ClosingExistingCheckoutHistory(assetId, now);

            // look for existing hold on the item
            var currentHolds = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == assetId);

            // if there are holds, checkout the item to the library card with the earliest hold
            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(assetId, currentHolds);
            }

            // otherwise update the item to available
            UpdateAssetStatus(assetId, "Available");

            _context.SaveChanges();
        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<AssetHoldModel> currentHolds)
        {
            var earliestHold = currentHolds
                .OrderBy(holds => holds.HoldPlaced)
                .FirstOrDefault();
            
            var card = earliestHold.LibraryCard;

            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckoutItem(assetId, card.Id);            
        }

        public void CheckoutItem(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            
            if (IsCheckedOut(assetId))
            {
                return;
                // add logic here to handle feedback to the user
            }

            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            UpdateAssetStatus(assetId, "Checked Out");

            var libraryCard = _context.LibraryCards
                .Include(card => card.Checkouts)
                .FirstOrDefault(card => card.Id == libraryCardId);

            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }
        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            
            if (checkout == null)
            {
                return "Not checked out yet";
            }

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                .Include(co => co.LibraryAsset)
                .Include(co => co.LibraryCard)
                .FirstOrDefault(co => co.LibraryAsset.Id == assetId);
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            var card = _context.LibraryCards
                .FirstOrDefault(c => c.Id == libraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }

            var hold = new AssetHoldModel 
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };

            _context.Add(hold);
            _context.SaveChanges();
        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId);

            var cardId = hold?.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId)
                .HoldPlaced;
        }

        // =================================================================== //
        // =================================================================== //
        // =================================================================== //
        // =================================================================== //

        private void UpdateAssetStatus(int assetId, string statusName)
        {
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);

            item.Status = _context.Statuses.FirstOrDefault(status => status.Name == statusName);
        }

        private void ClosingExistingCheckoutHistory(int assetId, DateTime now)
        {
            var history = _context.CheckoutHistories.FirstOrDefault(h => h.LibraryAsset.Id == assetId && h.CheckedIn == null);

            if ( history != null )
            {
                _context.Update(history);
                history.CheckedIn = DateTime.Now;
            }
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            var checkout = _context.Checkouts.FirstOrDefault(c => c.LibraryAsset.Id == assetId);

            if ( checkout != null )
            {
                _context.Remove(checkout);
            }
        }
        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        private bool IsCheckedOut(int assetId)
        {
            return _context.Checkouts
                .Where(co => co.LibraryAsset.Id == assetId)
                .Any();
        }
    }
}