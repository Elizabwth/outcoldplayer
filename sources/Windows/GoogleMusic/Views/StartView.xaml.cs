﻿//--------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
//--------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Views
{
    using System.Collections.Generic;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Presenters;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public interface IStartView : IView
    {
        void SetGroups(List<GroupBindingModel> groups);
    }

    public sealed partial class StartView : ViewBase, IStartView
    {
        public StartView()
        {
            this.InitializePresenter<StartViewPresenter>();
            this.InitializeComponent();
        }

        public void SetGroups(List<GroupBindingModel> groups)
        {
            this.Groups.Source = groups;
        }

        private void PlaylistItemClick(object sender, ItemClickEventArgs e)
        {
            this.Presenter<StartViewPresenter>().ItemClick(e.ClickedItem as PlaylistBindingModel);
        }

        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var groupBindingModel = frameworkElement.DataContext as GroupBindingModel;
                if (groupBindingModel != null)
                {
                    App.Container.Resolve<INavigationService>().NavigateTo<IPlaylistsView>(groupBindingModel.Request);
                }
            }
        }
    }
}
