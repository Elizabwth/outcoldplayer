﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    public interface IView
    {
        void OnNavigatedTo(NavigatedToEventArgs eventArgs);

        void OnNavigatingFrom(NavigatingFromEventArgs eventArgs);
    }
}