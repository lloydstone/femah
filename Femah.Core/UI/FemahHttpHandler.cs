﻿using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using Femah.Core;

namespace Femah.Ui
{
    internal sealed class FemahHttpHandler : IHttpHandler
    {
        private const string _enableFeatureAction = "enablefeature";
        private const string _setSwitchTypeAction = "setswitchtype";
        private const string _setCustomAttributesAction = "setcustomattributes";

        public void ProcessRequest(HttpContext context)
        {
            string name;

            var action = context.Request.QueryString["action"];
            if ( action != null )
            {
                switch(action)
                {
                    case _enableFeatureAction:
                        name = context.Request.QueryString["name"];
                        var enabled = context.Request.QueryString["enabled"];
                        bool doEnable;
                        if (name != null && enabled != null && Boolean.TryParse(enabled, out doEnable))
                        {
                            FeatureSwitching.EnableFeature(name, doEnable);
                        }
                        break;

                    case _setSwitchTypeAction:
                        name = context.Request.QueryString["name"];
                        var switchType = context.Request.QueryString["switchtype"];
                        if (name != null && switchType != null)
                        {
                            FeatureSwitching.SetSwitchType(name, switchType);
                        }
                        break;

                    case _setCustomAttributesAction:
                        name = context.Request.QueryString["name"];
                        FeatureSwitching.SetFeatureAttributes(name, context.Request.QueryString);
                        break;
                }

                context.Response.Redirect(context.Request.Url.AbsolutePath);
                return;
            }

            var stringWriter = new StringWriter();
            var htmlTextWriter = context.Request.Browser.CreateHtmlTextWriter(stringWriter);

            RenderPage(htmlTextWriter);

            context.Response.Write(stringWriter.ToString());
        }

        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Render the HTML for the admin page to the response.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to render the page to.</param>
        private void RenderPage( HtmlTextWriter writer )
        {
            writer.BeginRender();
            writer.RenderBeginTag(HtmlTextWriterTag.Html);

            // Write HTML header.
            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.Write("FEMAH");
            writer.RenderEndTag( /* Title */ );
            writer.RenderEndTag( /* Head */ );

            // Write page header.
            writer.RenderBeginTag(HtmlTextWriterTag.Body);
            writer.RenderBeginTag(HtmlTextWriterTag.H1);
            writer.Write("FEMAH");
            writer.RenderEndTag(/* H1 */);

            // Render all feature switches.
            var features = FeatureSwitching.AllFeatures();
            if (features.Count == 0)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.Write("There are no feature switches.");
                writer.RenderEndTag(/* P */);
            }
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                foreach (var feature in features.OrderBy( f => f.Name))
                {
                    RenderFeatureRow(writer, feature);
                }
                writer.RenderEndTag(/* Table */);
            }

            writer.RenderEndTag(/* Body */);
            writer.RenderEndTag(/* Html */);
            writer.EndRender();
        }

        /// <summary>
        /// Render a single feature row to the table of features.
        /// </summary>
        /// <param name="writer">A <see cref="HtmlTextWriter"/> to use to render the HTML.</param>
        /// <param name="feature">The <see cref="IFeatureSwitch"/> to render.</param>
        private void RenderFeatureRow(HtmlTextWriter writer, IFeatureSwitch feature)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Feature name.
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(feature.Name);
            writer.RenderEndTag(/* Td */);

            // Feature type.
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(feature.GetType().Name);

            var switchTypes = FeatureSwitching.AllSwitchTypes();
            if ( switchTypes.Count >= 2 )
            {
                writer.AddAttribute("action", "/femah.axd");
                writer.RenderBeginTag(HtmlTextWriterTag.Form);
                writer.Write(String.Format("<input type='hidden' name='action' value='{0}'></input>", _setSwitchTypeAction ));
                writer.Write(String.Format("<input type='hidden' name='name' value='{0}'></input>", feature.Name));
                writer.AddAttribute(HtmlTextWriterAttribute.Name, "switchtype");
                writer.RenderBeginTag(HtmlTextWriterTag.Select);
                foreach ( var switchType in switchTypes )
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, switchType.AssemblyQualifiedName);
                    writer.RenderBeginTag(HtmlTextWriterTag.Option);
                    writer.Write(switchType.Name);
                    writer.RenderEndTag(/* Option */);
                }
                writer.RenderEndTag(/* Select */);
                writer.Write("<input type='submit' value='Change'></input>");
                writer.RenderEndTag(/* Form */);
            }
            writer.RenderEndTag(/* Td */);

            // Enabled or disabled.
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(feature.IsEnabled ? "Enabled" : "Disabled");

            writer.AddAttribute("action","/femah.axd");
            writer.RenderBeginTag(HtmlTextWriterTag.Form);
            writer.Write(String.Format("<input type='hidden' name='action' value='{0}'></input>", _enableFeatureAction));
            writer.Write(String.Format("<input type='hidden' name='name' value='{0}'></input>", feature.Name));
            writer.Write(String.Format("<input type='hidden' name='enabled' value='{0}'></input>", !feature.IsEnabled));
            writer.Write(String.Format("<input type='submit' value='{0}'></input>", feature.IsEnabled ? "Disable" : "Enable"));
            writer.RenderEndTag(/* Form */);
            writer.RenderEndTag(/* Td */);

            // Custom attributes.
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.RenderBeginTag(HtmlTextWriterTag.Form);
            writer.Write(String.Format("<input type='hidden' name='name' value='{0}'></input>", feature.Name));
            writer.Write(String.Format("<input type='hidden' name='action' value='{0}'></input>", _setCustomAttributesAction));
            feature.RenderUI(writer);
            writer.RenderEndTag(/* Form*/);
            writer.RenderEndTag(/* Td */);

            // Exceptions associated with feature switch.
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            var exceptionList = FeatureSwitching.GetFeatureSwitchExceptions(feature.Name);
            if (exceptionList.Count > 0)
            {
                writer.Write(String.Format("{0} exception(s) have been thrown by this feature switch.", exceptionList.Count));
            } 
            writer.RenderEndTag(/* Td */);

            writer.RenderEndTag(/* Tr */);
        }
    }
}
