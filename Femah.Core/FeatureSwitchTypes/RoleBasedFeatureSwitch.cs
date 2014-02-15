using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Femah.Core.FeatureSwitchTypes
{
    public class RoleBasedFeatureSwitch : FeatureSwitchBase
    {
        public RoleBasedFeatureSwitch()
        {
            AcceptedRoles = new List<string>();
        }

        /// <summary>
        /// Determine if feature is on or off, according to the current user's roles.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if feature is on, false otherwise.</returns>
        public override bool IsOn(IFemahContext context)
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

        public override void RenderUI(System.Web.UI.HtmlTextWriter writer)
        {
            writer.Write(String.Format("Roles: <input type='text' name='roles' value='{0}' /><input type='submit' value='update'/>", String.Join(",",AcceptedRoles.ToArray())));
            return;
        }

        public override void SetCustomAttributes(NameValueCollection values)
        {
            this.AcceptedRoles.Clear();

            string roleList = values["roles"];
            if (!String.IsNullOrEmpty(roleList))
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
