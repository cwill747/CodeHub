using System.Collections.Generic;
using System.Windows.Input;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.User;
using System.Linq;
using CodeHub.Core.Utils;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Notifications;
using GitHubSharp.Models;
using MvvmCross.Plugins.Messenger;
using MvvmCross.Core.ViewModels;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApplicationService _applicationService;
        private readonly IFeaturesService _featuresService;
        private int _notifications;
        private List<BasicUserModel> _organizations;
        private readonly MvxSubscriptionToken _notificationCountToken;

        public int Notifications
        {
            get { return _notifications; }
            set { _notifications = value; RaisePropertyChanged(); }
        }

        public List<BasicUserModel> Organizations
        {
            get { return _organizations; }
            set { _organizations = value; RaisePropertyChanged(); }
        }
        
        public Account Account
        {
            get { return _applicationService.Account; }
        }

        public bool ShouldShowUpgrades
        {
            get { return !_featuresService.IsProEnabled; }
        }
        
        public MenuViewModel(IApplicationService application, IFeaturesService featuresService)
        {
            _applicationService = application;
            _featuresService = featuresService;
            _notificationCountToken = Messenger.SubscribeOnMainThread<NotificationCountMessage>(OnNotificationCountMessage);
        }

        private void OnNotificationCountMessage(NotificationCountMessage msg)
        {
            Notifications = msg.Count;
        }

        public IReactiveCommand<object> GoToAccountsCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Profile")]
        public ICommand GoToProfileCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserViewModel>(new UserViewModel.NavObject { Username = _applicationService.Account.Username })); }
        }

        [PotentialStartupViewAttribute("Notifications")]
        public ICommand GoToNotificationsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NotificationsViewModel>(null)); }
        }

        [PotentialStartupViewAttribute("My Issues")]
        public ICommand GoToMyIssuesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<MyIssuesViewModel>(null)); }
        }

        [PotentialStartupViewAttribute("My Events")]
        public ICommand GoToMyEvents
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserEventsViewModel>(new UserEventsViewModel.NavObject { Username = Account.Username })); }
        }

        [PotentialStartupViewAttribute("My Gists")]
        public ICommand GoToMyGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserGistsViewModel>(new UserGistsViewModel.NavObject { Username = Account.Username}));}
        }

        [PotentialStartupViewAttribute("Starred Gists")]
        public ICommand GoToStarredGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<StarredGistsViewModel>(null)); }
        }

        [PotentialStartupViewAttribute("Public Gists")]
        public ICommand GoToPublicGistsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<PublicGistsViewModel>(null)); }
        }

        [PotentialStartupViewAttribute("Starred Repositories")]
        public ICommand GoToStarredRepositoriesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesStarredViewModel>(null));}
        }

        [PotentialStartupViewAttribute("Owned Repositories")]
        public ICommand GoToOwnedRepositoriesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UserRepositoriesViewModel>(new UserRepositoriesViewModel.NavObject { Username = Account.Username }));}
        }

        [PotentialStartupViewAttribute("Explore Repositories")]
        public ICommand GoToExploreRepositoriesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesExploreViewModel>(null));}
        }

        [PotentialStartupViewAttribute("Trending Repositories")]
        public ICommand GoToTrendingRepositoriesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<RepositoriesTrendingViewModel>(null));}
        }

        public ICommand GoToOrganizationEventsCommand
        {
            get { return new MvxCommand<string>(x => ShowMenuViewModel<Events.UserEventsViewModel>(new Events.UserEventsViewModel.NavObject { Username = x }));}
        }

        public ICommand GoToOrganizationCommand
        {
            get { return new MvxCommand<string>(x => ShowMenuViewModel<Organizations.OrganizationViewModel>(new Organizations.OrganizationViewModel.NavObject { Name = x }));}
        }

        [PotentialStartupViewAttribute("Organizations")]
        public ICommand GoToOrganizationsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<Organizations.OrganizationsViewModel>(new Organizations.OrganizationsViewModel.NavObject { Username = Account.Username }));}
        }

        [DefaultStartupViewAttribute]
        [PotentialStartupViewAttribute("News")]
        public ICommand GoToNewsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<NewsViewModel>(null));}
        }

        public ICommand GoToSettingsCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<SettingsViewModel>(null));}
        }

        public ICommand GoToSupport
        {
            get { return new MvxCommand(() => ShowMenuViewModel<WebBrowserViewModel>(new WebBrowserViewModel.NavObject { Url = "https://codehub.uservoice.com/" })); }
        }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand<RepositoryIdentifier>(x => ShowMenuViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner, Repository = x.Name }));}
        }

        public ICommand GoToUpgradesCommand
        {
            get { return new MvxCommand(() => ShowMenuViewModel<UpgradeViewModel>(null)); }
        }

        public ICommand LoadCommand
        {
            get { return new MvxCommand(Load);}    
        }

        private void Load()
        {
            var notificationRequest = this.GetApplication().Client.Notifications.GetAll();
            this.GetApplication().Client.ExecuteAsync(notificationRequest)
                .ToBackground(x => Notifications = x.Data.Count);

            var organizationsRequest = this.GetApplication().Client.AuthenticatedUser.GetOrganizations();
            this.GetApplication().Client.ExecuteAsync(organizationsRequest)
                .ToBackground(x => Organizations = x.Data.ToList());
        }

        //
        //        private async Task PromptForPushNotifications()
        //        {
        //            // Push notifications are not enabled for enterprise
        //            if (Account.IsEnterprise)
        //                return;
        //
        //            try
        //            {
        //                var features = Mvx.Resolve<IFeaturesService>();
        //                var alertDialog = Mvx.Resolve<IAlertDialogService>();
        //                var push = Mvx.Resolve<IPushNotificationsService>();
        //                var 
        //                // Check for push notifications
        //                if (Account.IsPushNotificationsEnabled == null && features.IsPushNotificationsActivated)
        //                {
        //                    var result = await alertDialog.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
        //                    if (result)
        //                        Task.Run(() => push.Register()).FireAndForget();
        //                    Account.IsPushNotificationsEnabled = result;
        //                    Accounts.Update(Account);
        //                }
        //                else if (Account.IsPushNotificationsEnabled.HasValue && Account.IsPushNotificationsEnabled.Value)
        //                {
        //                    Task.Run(() => push.Register()).FireAndForget();
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                _alertDialogService.Alert("Error", e.Message);
        //            }
        //        }

        private static readonly IDictionary<string, string> Presentation = new Dictionary<string, string> { { PresentationValues.SlideoutRootPresentation, string.Empty } };

        public ICommand GoToDefaultTopView
        {
            get
            {
                var startupViewName = Account.DefaultStartupView;
                if (!string.IsNullOrEmpty(startupViewName))
                {
                    var props = from p in GetType().GetProperties()
                                let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true)
                                where attr.Length == 1
                                select new { Property = p, Attribute = attr[0] as PotentialStartupViewAttribute };

                    foreach (var p in props)
                    {
                        if (string.Equals(startupViewName, p.Attribute.Name))
                            return p.Property.GetValue(this) as ICommand;
                    }
                }

                //Oh no... Look for the last resort DefaultStartupViewAttribute
                var deprop = (from p in GetType().GetProperties()
                              let attr = p.GetCustomAttributes(typeof(DefaultStartupViewAttribute), true)
                              where attr.Length == 1
                              select new { Property = p, Attribute = attr[0] as DefaultStartupViewAttribute }).FirstOrDefault();

                //That shouldn't happen...
                if (deprop == null)
                    return null;
                var val = deprop.Property.GetValue(this);
                return val as ICommand;
            }
        }

        public ICommand DeletePinnedRepositoryCommand
        {
            get
            {
                return new MvxCommand<PinnedRepository>(x =>
                {
                    Account.PinnedRepositories.Remove(x);
                    _applicationService.UpdateActiveAccount().ToBackground();
                }, x => x != null);
            }
        }

        protected bool ShowMenuViewModel<T>(object data) where T : IMvxViewModel
        {
            return this.ShowViewModel<T>(data, new MvxBundle(Presentation));
        }

        public IEnumerable<PinnedRepository> PinnedRepositories
        {
            get { return Account.PinnedRepositories; }
        }
    }
}
