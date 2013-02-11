﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Views
{
    using Windows.UI.Xaml.Controls;

    public class PageBase : Page
    {
        private readonly IDependencyResolverContainer container;

        public PageBase()
        {
            this.container = ApplicationBase.Container;
        }

        protected TPresenter InitializePresenter<TPresenter>() where TPresenter : PresenterBase
        {
            var presenter = this.container.Resolve<TPresenter>(new object[] { this });
            this.DataContext = presenter;
            return presenter;
        }
    }
}