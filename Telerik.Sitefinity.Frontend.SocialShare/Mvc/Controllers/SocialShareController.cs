﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using ServiceStack.Text;
using Telerik.Sitefinity.Frontend.SocialShare.Mvc.Models;
using Telerik.Sitefinity.Mvc;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.SiteSettings.Basic;

namespace Telerik.Sitefinity.Frontend.SocialShare.Mvc.Controllers
{
    /// <summary>
    /// Social Share
    /// </summary>
    [ControllerToolboxItem(Name = "SocialShare", Title = "Social share", SectionName = "MvcWidgets")]
    public class SocialShareController : Controller
    {
        public SocialShareController()
        {
        }

        #region Actions
        /// <summary>
        /// Default Action
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var model = new SocialShareModel();

            return this.View("SocialShare", model);
        }

        #endregion

        #region Overridden methods

        /// <summary>
        /// Called when a request matches this controller, but no method with the specified action name is found in the controller.
        /// </summary>
        /// <param name="actionName">The name of the attempted action.</param>
        protected override void HandleUnknownAction(string actionName)
        {
            this.Index().ExecuteResult(this.ControllerContext);
        }

        #endregion

        /// <summary>
        /// Gets or sets the social share section map.
        /// </summary>
        /// <value>The social share section map.</value>
        [Browsable(false)]
        internal IList<SocialShareMap> SocialShareSectionMap
        {
            get
            {
                var socialShareSettings = SystemManager.CurrentContext.GetSetting<SocialShareSettingsContract, ISocialShareSettings>();

                var socialShareSectionMap = new List<SocialShareMap>();

                socialShareSectionMap.Add(new SocialShareMap(new Dictionary<string, bool> 
                { 
                    { "Facebook", socialShareSettings.Facebook },
                    { "Twitter", socialShareSettings.Twitter },
                    { "Google +", socialShareSettings.GooglePlusOne },
                    { "LinkedIn", socialShareSettings.LinkedIn },
                    { "Digg", socialShareSettings.Digg }
                }));

                socialShareSectionMap.Add(new SocialShareMap(new Dictionary<string, bool> 
                { 
                    { "Blogger", socialShareSettings.Blogger },
                    { "Tumblr", socialShareSettings.Tumblr },
                    { "Google bookmarks", socialShareSettings.GoogleBookmarks },
                    { "Delicious", socialShareSettings.Delicious },
                    { "My Space", socialShareSettings.MySpace }
                }));

                socialShareSectionMap.Add(new SocialShareMap(new Dictionary<string, bool> 
                { 
                    { "Stumble upon", socialShareSettings.StumbleUpon },
                    { "Reddit", socialShareSettings.Reddit },
                    { "MailTo", socialShareSettings.MailTo }
                }));

                return socialShareSectionMap;
            }
        }

        /// <summary>
        /// Gets or sets the serialize social share section map.
        /// </summary>
        /// <value>The serialize social share section map.</value>
        public string SerializeSocialShareSectionMap
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.serializeSocialShareSectionMap))
                {
                    this.serializeSocialShareSectionMap = JsonSerializer.SerializeToString(this.SocialShareSectionMap);
                }

                return this.serializeSocialShareSectionMap;
            }

            set
            {
                if (this.serializeSocialShareSectionMap != value)
                {
                    this.serializeSocialShareSectionMap = value;
                }
            }
        }

        private string serializeSocialShareSectionMap;
    }
}
