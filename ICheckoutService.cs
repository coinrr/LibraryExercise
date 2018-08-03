using System;
using System.Collections.Generic;
using LibraryExercise.Models;

namespace LibraryExercise
{
    public interface ICheckoutService
    {
         void Add(Checkout newCheckout);

         IEnumerable<Checkout> GetAll();
         IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
         IEnumerable<Hold> GetCurrentHolds(int id);

         Checkout GetById(int checkoutId);
         Checkout GetLatestCheckout(int assetId);
         string GetCurrentCheckoutPatron(int assetId);
         string GetCurrentHoldPatronName(int holdId);
         DateTime GetCurrentHoldPlaced(int holdId);

         void CheckoutItem(int assetId, int libraryCardId);
         void CheckinItem(int assetId, int libraryCardId);
         void PlaceHold(int assetId, int libraryCardId);
         void MarkLost(int assetId);
         void MarkFound(int assetId);
    }
}