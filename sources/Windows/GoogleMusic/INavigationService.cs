﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic
{
    using OutcoldSolutions.GoogleMusic.Views;

    public interface INavigationService
    {
        TView NavigateTo<TView>(object parameter = null, bool keepInHistory = true) where TView : IView;

        void GoBack();

        bool CanGoBack();
    }
}