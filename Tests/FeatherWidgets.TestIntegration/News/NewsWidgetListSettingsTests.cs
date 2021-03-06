﻿using System.IO;
using System.Linq;
using FeatherWidgets.TestUtilities.CommonOperations;
using FeatherWidgets.TestUtilities.CommonOperations.Templates;
using MbUnit.Framework;
using Telerik.Sitefinity.Frontend.Mvc.Models;
using Telerik.Sitefinity.Frontend.News.Mvc.Controllers;
using Telerik.Sitefinity.Frontend.TestUtilities;
using Telerik.Sitefinity.Modules.News;
using Telerik.Sitefinity.Mvc.Proxy;
using Telerik.Sitefinity.News.Model;
using Telerik.Sitefinity.Web;

namespace FeatherWidgets.TestIntegration.News
{
    /// <summary>
    /// This is a class with News tests.
    /// </summary>
    [TestFixture]
    public class NewsWidgetListSettingsTests
    {
        /// <summary>
        /// Set up method
        /// </summary>
        [SetUp]
        public void Setup()
        {
            this.pageOperations = new PagesOperations();
        }

        /// <summary>
        /// Tears down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().DeleteAllNews();
        }

        /// <summary>
        /// News widget - test Use paging functionality 
        /// </summary>
        [Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifyUsePagingFunctionality()
        {
            string testName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            string pageNamePrefix = testName + "NewsPage";
            string pageTitlePrefix = testName + "News Page";
            string urlNamePrefix = testName + "news-page";
            int index = 1;
            string index2 = "/2";
            int itemsPerPage = 3;
            string url = UrlPath.ResolveAbsoluteUrl("~/" + urlNamePrefix + index);
            string url2 = UrlPath.ResolveAbsoluteUrl("~/" + urlNamePrefix + index + index2);

            var mvcProxy = new MvcControllerProxy();
            mvcProxy.ControllerName = typeof(NewsController).FullName;
            var newsController = new NewsController();
            newsController.Model.ItemsPerPage = itemsPerPage;
            mvcProxy.Settings = new Telerik.Sitefinity.Mvc.Proxy.ControllerSettings(newsController);

            try
            {
                this.pageOperations.CreatePageWithControl(mvcProxy, pageNamePrefix, pageTitlePrefix, urlNamePrefix, index);

                int newsCount = 5;

                for (int i = 1; i <= newsCount; i++)
                    Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(NewsTitle + i);

                string responseContent = PageInvoker.ExecuteWebRequest(url);
                string responseContent2 = PageInvoker.ExecuteWebRequest(url2);

                for (int i = 1; i <= newsCount; i++)
                {
                    if (i <= 2)
                    {
                        Assert.IsFalse(responseContent.Contains(NewsTitle + i), "The news with this title was found!");
                    }
                    else
                    {
                        Assert.IsTrue(responseContent.Contains(NewsTitle + i), "The news with this title was not found!");
                    }
                }

                for (int i = 1; i <= newsCount; i++)
                {
                    if (i <= 2)
                    {
                        Assert.IsTrue(responseContent2.Contains(NewsTitle + i), "The news with this title was not found!");
                    }
                    else
                    {
                        Assert.IsFalse(responseContent2.Contains(NewsTitle + i), "The news with this title was found!");
                    }
                }

                Assert.IsTrue(responseContent.Contains("<link rel=\"next\""), "Canonical pagination URL for Next was not found!");
                Assert.IsTrue(responseContent2.Contains("<link rel=\"prev\""), "Canonical pagination URL for Next was not found!");
            }
            finally
            {
                this.pageOperations.DeletePages();
            }
        }

        /// <summary>
        /// News widget - test No limit and paging functionality 
        /// </summary>
        [Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifyNoLimitAndPagingFunctionality()
        {
            string testName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            string pageNamePrefix = testName + "NewsPage";
            string pageTitlePrefix = testName + "News Page";
            string urlNamePrefix = testName + "news-page";
            int index = 1;
            string url = UrlPath.ResolveAbsoluteUrl("~/" + urlNamePrefix + index);

            var mvcProxy = new MvcControllerProxy();
            mvcProxy.ControllerName = typeof(NewsController).FullName;
            var newsController = new NewsController();
            newsController.Model.DisplayMode = ListDisplayMode.All;
            mvcProxy.Settings = new Telerik.Sitefinity.Mvc.Proxy.ControllerSettings(newsController);

            try
            {
                this.pageOperations.CreatePageWithControl(mvcProxy, pageNamePrefix, pageTitlePrefix, urlNamePrefix, index);

                int newsCount = 25;

                for (int i = 1; i <= newsCount; i++)
                    Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(NewsTitle + i);

                string responseContent = PageInvoker.ExecuteWebRequest(url);

                for (int i = 1; i <= newsCount; i++)
                    Assert.IsTrue(responseContent.Contains(NewsTitle + i), "The news with this title was not found!");
            }
            finally
            {
                this.pageOperations.DeletePages();
            }
        }

        /// <summary>
        /// News widget - test Use limit functionality 
        /// </summary>
        [Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifyUseLimitFunctionality()
        {
            string testName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            string pageNamePrefix = testName + "NewsPage";
            string pageTitlePrefix = testName + "News Page";
            string urlNamePrefix = testName + "news-page";
            int index = 1;
            int limitCount = 3;
            string url = UrlPath.ResolveAbsoluteUrl("~/" + urlNamePrefix + index);

            var mvcProxy = new MvcControllerProxy();
            mvcProxy.ControllerName = typeof(NewsController).FullName;
            var newsController = new NewsController();
            newsController.Model.DisplayMode = ListDisplayMode.Limit;
            newsController.Model.LimitCount = limitCount;
            mvcProxy.Settings = new Telerik.Sitefinity.Mvc.Proxy.ControllerSettings(newsController);

            try
            {
                this.pageOperations.CreatePageWithControl(mvcProxy, pageNamePrefix, pageTitlePrefix, urlNamePrefix, index);

                int newsCount = 5;

                for (int i = 1; i <= newsCount; i++)
                    Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(NewsTitle + i);

                string responseContent = PageInvoker.ExecuteWebRequest(url);

                for (int i = 5; i <= limitCount; i--)
                    Assert.IsTrue(responseContent.Contains(NewsTitle + i), "The news with this title was not found!");
            }
            finally
            {
                this.pageOperations.DeletePages();
            }
        }

        /// <summary>
        /// News widget - test Sort news functionality 
        /// </summary>
        [Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifySortNewsAscending()
        {
            string sortExpession = "Title ASC";
            string[] newsTitles = { "Cat", "Boat", "Angel" };

            var newsController = new NewsController();
            newsController.Model.DisplayMode = ListDisplayMode.Limit;
            newsController.Model.SortExpression = sortExpession;

            for (int i = 0; i < newsTitles.Length; i++)
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(newsTitles[i]);

            var items = newsController.Model.CreateListViewModel(null, 1).Items.ToArray();

            int lastIndex = newsTitles.Length - 1;
            for (int i = 0; i < newsTitles.Length; i++)
            { 
                Assert.IsTrue(items[i].Fields.Title.Equals(newsTitles[lastIndex]), "The news with this title was not found!");
                lastIndex--;
            }
        }

        /// <summary>
        /// Newses the widget_ verify sort news descending.
        /// </summary>
        [Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifySortNewsDescending()
        {
            string sortExpession = "Title DESC";
            string[] newsTitles = { "Cat", "Boat", "Angel" };

            var newsController = new NewsController();
            newsController.Model.DisplayMode = ListDisplayMode.Limit;
            newsController.Model.SortExpression = sortExpession;

            for (int i = 0; i < newsTitles.Length; i++)
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(newsTitles[i]);

            var items = newsController.Model.CreateListViewModel(null, 1).Items.ToArray();

            int lastIndex = 0;
            for (int i = 0; i < newsTitles.Length; i++)
            {
                Assert.IsTrue(items[i].Fields.Title.Equals(newsTitles[lastIndex]), "The news with this title was not found!");
                lastIndex++;
            }
        }

        /// <summary>
        /// Newses the widget_ verify sort news by publication date descending.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifySortNewsPublicationDateDescending()
        {
            string sortExpession = "PublicationDate DESC";
            string[] newsTitles = { "Cat", "Boat", "Angel" };

            var newsController = new NewsController();
            newsController.Model.DisplayMode = ListDisplayMode.Limit;
            newsController.Model.SortExpression = sortExpession;

            for (int i = 0; i < newsTitles.Length; i++)
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(newsTitles[i]);

            var items = newsController.Model.CreateListViewModel(null, 1).Items.ToArray();

            int lastIndex = newsTitles.Length - 1;

            var newsManager = NewsManager.GetManager();
            var newsItems = newsManager.GetNewsItems().Where<NewsItem>(ni => ni.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master).OrderBy(ni => ni.PublicationDate);
            
            foreach (NewsItem item in newsItems)
            {
                Assert.IsTrue(items[lastIndex].Fields.PublicationDate.Equals(item.PublicationDate), "The news with this title was not found!");
                lastIndex--;
            }
        }

        /// <summary>
        /// Newses the widget_ verify sort news by last modified date descending.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_VerifySortNewsLastModifiedDateDescending()
        {
            string sortExpession = "LastModified DESC";
            string[] newsTitles = { "Cat", "Boat", "Angel" };

            var newsController = new NewsController();
            newsController.Model.DisplayMode = ListDisplayMode.Limit;
            newsController.Model.SortExpression = sortExpession;
            var newsManager = NewsManager.GetManager();

            for (int i = 0; i < newsTitles.Length; i++)
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(newsTitles[i]);

            NewsItem modified = newsManager.GetNewsItems().Where<NewsItem>(ni => ni.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master && ni.Title == "Boat").FirstOrDefault();

            NewsItem temp = newsManager.Lifecycle.CheckOut(modified) as NewsItem;

            temp.Title = "BoatNew";

            modified = newsManager.Lifecycle.CheckIn(temp) as NewsItem;

            var newsItem = newsManager.GetNewsItem(modified.Id);
            newsManager.Lifecycle.Publish(newsItem);
            newsManager.SaveChanges();

            var items = newsController.Model.CreateListViewModel(null, 1).Items.ToArray();
    
            Assert.IsTrue(items[0].Fields.LastModified.Equals(modified.LastModified), "The news with this title was not found!");
            Assert.IsTrue(items[0].Fields.Title.Equals(modified.Title.Value), "The news with this title was not found!");
            Assert.IsTrue(items[1].Fields.Title.Equals(newsTitles[2]), "The news with this title was not found!");
            Assert.IsTrue(items[2].Fields.Title.Equals(newsTitles[0]), "The news with this title was not found!");
        }

        [Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        public void NewsWidget_SelectListTemplate()
        {
            string testName = System.Reflection.MethodInfo.GetCurrentMethod().Name;
            string pageNamePrefix = testName + "NewsPage";
            string pageTitlePrefix = testName + "News Page";
            string urlNamePrefix = testName + "news-page";
            int pageIndex = 1;
            string textEdited = "<p> Test paragraph </p>";
            string paragraphText = "Test paragraph";
            string url = UrlPath.ResolveAbsoluteUrl("~/" + urlNamePrefix + pageIndex);

            string listTemplate = "NewsListNew";
            var listTemplatePath = Path.Combine(this.templateOperation.SfPath, "ResourcePackages", "Bootstrap", "MVC", "Views", "News", "List.NewsList.cshtml");
            var newListTemplatePath = Path.Combine(this.templateOperation.SfPath, "MVC", "Views", "Shared", "List.NewsListNew.cshtml");

            try
            {
                File.Copy(listTemplatePath, newListTemplatePath);

                using (StreamWriter output = File.AppendText(newListTemplatePath))
                {
                    output.WriteLine(textEdited);
                }

                var mvcProxy = new MvcControllerProxy();
                mvcProxy.ControllerName = typeof(NewsController).FullName;
                var newsController = new NewsController();
                newsController.ListTemplateName = listTemplate;
                mvcProxy.Settings = new ControllerSettings(newsController);

                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem(NewsTitle);

                this.pageOperations.CreatePageWithControl(mvcProxy, pageNamePrefix, pageTitlePrefix, urlNamePrefix, pageIndex);

                string responseContent = PageInvoker.ExecuteWebRequest(url);

                Assert.IsTrue(responseContent.Contains(paragraphText), "The news with this template was not found!");
            }
            finally
            {
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.Pages().DeleteAllPages();
                File.Delete(newListTemplatePath);
            }
        }

        /// <summary>
        /// Verifies news items sorted by a valid As set in Advanced mode option.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)"), Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        [Description("Verifies news items sorted by a valid As set in Advanced mode option.")]
        public void NewsWidget_VerifyValidSortingOptionAsSetInAdvancedMode()
        {
            string sortExpession = "InvalidSortingExpression";
            var itemsCount = 5;
            string[] expectedSortedNewsTitles = { "news4", "news3", "news2", "news1", "news0" };

            var newsController = new NewsController();
            newsController.Model.SortExpression = sortExpession;

            for (int i = 0; i < itemsCount; i++)
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem("news" + i);

            var items = newsController.Model.CreateListViewModel(null, 1).Items.ToArray();

            for (int i = 0; i < itemsCount; i++)
            {
                Assert.AreEqual(expectedSortedNewsTitles[i], items[i].Fields.Title.Value, "The news with this title was not found!");
            }
        }

        /// <summary>
        /// Verifies news items sorted by an invalid As set in Advanced mode option.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.IndexOf(System.String)"), Test]
        [Category(TestCategories.News)]
        [Author(FeatherTeams.SitefinityTeam7)]
        [Description("Verifies news items sorted by an invalid As set in Advanced mode option.")]
        public void NewsWidget_VerifyInvalidSortingOptionAsSetInAdvancedMode()
        {
            string sortExpession = "Content ASC";
            var itemsCount = 5;
            string[] newsContent = { "Ivan", "George", "Steve", "Ana", "Tom" };
            string[] expectedSortedNewsTitles = { "news3", "news1", "news0", "news2", "news4" };

            var newsController = new NewsController();
            newsController.Model.SortExpression = sortExpession;

            for (int i = 0; i < itemsCount; i++)
                Telerik.Sitefinity.TestUtilities.CommonOperations.ServerOperations.News().CreateNewsItem("news" + i, content: newsContent[i]);

            var items = newsController.Model.CreateListViewModel(null, 1).Items.ToArray();

            for (int i = 0; i < itemsCount; i++)
            {
                Assert.AreEqual(expectedSortedNewsTitles[i], items[i].Fields.Title.Value, "The news with this title was not found!");
            }
        }

        #region Fields and constants

        private const string NewsTitle = "Title";
        private PagesOperations pageOperations;
        private TemplateOperations templateOperation = new TemplateOperations();
        
        #endregion
    }
}