using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femah.Core.FeatureSwitchTypes
{
    public class RoleBasedFeatureSwitch : IFeatureSwitch
    {
        public RoleBasedFeatureSwitch()
        {
            AcceptedRoles = new List<string>();
        }

        public bool IsEnabled { get;set;}

        public string Name { get; set; }

        public string FeatureType { get; set; }

        public bool IsOn(IFemahContext context)
        {
            if (context.HttpContext == null || context.HttpContext.User == null)
            {
                return false;
            }

            if ( AcceptedRoles == null || AcceptedRoles.Count == 0 )
            {
                return false;
            }

            foreach ( var role in AcceptedRoles )
            {
                if ( context.HttpContext.User.IsInRole(role) )
                {
                    return true;
                }
            }

            return false;
        }

        public void RenderUI(System.Web.UI.HtmlTextWriter writer)
        {

            writer.Write(String.Format("Roles: <input type='text' name='roles' value='{0}' /><input type='submit' value='update'/>", String.Join(",",AcceptedRoles.ToArray())));
            return;
        }

        public void SetCustomAttributes(NameValueCollection values)
        {
            this.AcceptedRoles.Clear();

            string roleList = values["roles"];
            if (!String.IsNullOrWhiteSpace(roleList))
            {
                foreach (var role in roleList.Split(','))
                {
                    this.AcceptedRoles.Add(role.Trim());
                }
            }
        }

        /// <summary>
        /// The roles who are able to see the feature.
        /// </summary>
        public List<string> AcceptedRoles { get; set; }
    }
}
