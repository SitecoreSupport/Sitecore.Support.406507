using System;
using Sitecore.Buckets.Util;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;

namespace Sitecore.Support.Buckets.Managers
{
    public class PipelineBasedBucketProvider : Sitecore.Buckets.Managers.PipelineBasedBucketProvider
    {
        public override Item CreateAndReturnBucketFolderDestination(Item topParent, DateTime childItemCreationDateTime, ID itemToMove,
            string itemName, ID templateId)
        {
            Assert.ArgumentNotNull(topParent, "topParent");
            Assert.ArgumentNotNull(childItemCreationDateTime, "childItemCreationDateTime");
            Database database = topParent.Database;
            Type type = Type.GetType(BucketConfigurationSettings.DynamicBucketFolderPath);
            IDynamicBucketFolderPath dynamicBucketFolderPath = ReflectionUtility.CreateInstance(type) as IDynamicBucketFolderPath;
            string str = dynamicBucketFolderPath.GetFolderPath(database, itemName, templateId, itemToMove, topParent.ID, childItemCreationDateTime);
            if (BucketConfigurationSettings.BucketFolderPath == string.Empty && dynamicBucketFolderPath is DateBasedFolderPath)
            {
                str = "Repository";
            }
            string path = topParent.Paths.FullPath + Sitecore.Buckets.Util.Constants.ContentPathSeperator + str;
            Item item;
            if ((item = database.GetItem(path)) == null)
            {
                TemplateItem templateItem = database.Templates[new TemplateID(BucketConfigurationSettings.ContainerTemplateId)];
                using (new EventDisabler())
                {
                    using (new SecurityDisabler())
                    {
                        item = database.CreateItemPath(path, templateItem, templateItem);
                    }
                }
            }
            Assert.IsNotNull(item, "Cannot resolve date folder destination.");
            if (!item.Axes.IsDescendantOf(topParent))
            {
                Log.Warn($"Your item {itemToMove} was placed in the wrong bucket due to two sibling items having the same name.", this);
            }
            return item;
        }
    }
}