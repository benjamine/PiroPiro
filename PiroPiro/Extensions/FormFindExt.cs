using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using System.Text.RegularExpressions;

namespace PiroPiro
{
    /// <summary>
    /// Extension methods to find diferent elements on html forms
    /// </summary>
    public static class FormFindExt
    {
        private static Regex InputNameRegex = new Regex(@"^[A-Za-z0-9_\-\.\[\]\+]+$", RegexOptions.Compiled);

        private static bool LooksLikeInputName(string selector)
        {
            return InputNameRegex.IsMatch(selector);
        }

        /// <summary>
        /// Finds an input field based on a label, name or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="labelNameOrSelector">label, name or css selector</param>
        /// <returns></returns>
        public static Element Field(this Element element, string labelNameOrSelector)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {
                Element field = null;

                if (FindExt.LooksLikeACssSelector(labelNameOrSelector))
                {
                    // find by css selector
                    field = element.Query(labelNameOrSelector).SingleOrDefault(e => e.IsField());
                    if (field != null)
                    {
                        return field;
                    }
                }

                if (LooksLikeInputName(labelNameOrSelector))
                {
                    // find by name
                    field = element.Query(string.Format("input[name='{0}'],select[name='{0}'],textarea[name='{0}']",
                        labelNameOrSelector))
                        .SingleOrDefault(e => e.IsField());
                    if (field != null)
                    {
                        return field;
                    }
                }

                // find by label
                var label = element.Query(string.Format("label:contains('{0}')", labelNameOrSelector)).SingleOrDefault();
                if (label == null)
                {
                    if (element.Browser.Configuration.GetFlag("FindFieldsByTableRows", false) ?? false)
                    {
                        // fallback: try to find fields by table rows
                        #region FindByRow

                        // fallback, try to find by table row
                        var tr = element.Query("tr:contains('" + labelNameOrSelector + "'):last").SingleOrDefault();
                        if (tr != null)
                        {
                            bool nameFound = false;
                            foreach (var td in tr.Query("td,th"))
                            {
                                if (!nameFound)
                                {
                                    nameFound = td.Text.Contains(labelNameOrSelector);
                                }
                                if (nameFound)
                                {
                                    // find the first field in this row, after label text
                                    field = td.Query("input,select,textarea").FirstOrDefault(e => e.IsField());
                                    if (field != null)
                                    {
                                        return field;
                                    }
                                }
                            }
                        }

                        #endregion
                    }

                    // no label found, try to detect bad semantics
                    var elem = element.Query(string.Format(":contains('{0}'):last", labelNameOrSelector)).SingleOrDefault();
                    if (elem != null)
                    {
                        throw new ElementNotFoundException(string.Format("text '{0}' was found in a {1} element, use label elements for fields", labelNameOrSelector, elem.TagName));
                    }
                }
                else
                {
                    string labelFor = label.GetAttribute("for");
                    if (!string.IsNullOrWhiteSpace(labelFor))
                    {
                        // for attribute is field id 
                        field = element.Query('#' + labelFor).SingleOrDefault();
                        if (field == null)
                        {
                            throw new ElementNotFoundException(string.Format("label '{0}' for is '{1}', but no element with that id could be found", labelNameOrSelector, labelFor));
                        }
                        if (!field.IsField())
                        {
                            throw new ElementNotFoundException(string.Format("label '{0}' for is '{1}', but element with that id is not a field", labelNameOrSelector, labelFor));
                        }
                    }
                    else
                    {
                        // find field inside label
                        field = label.Query("input,select,textarea").SingleOrDefault(e => e.IsField());
                        if (field == null)
                        {
                            throw new ElementNotFoundException(string.Format("label '{0}' has no for attribute and no field inside could be found", labelNameOrSelector, labelFor));
                        }
                    }
                }

                if (field == null)
                {
                    throw new ElementNotFoundException(string.Format("Field '{0}' not found", labelNameOrSelector));
                }
                return field;
            });
        }

        /// <summary>
        /// Finds a button or link (button, input[type=button|reset|submit] or a) by caption or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="captionOrSelector">caption or css selector</param>
        /// <returns></returns>
        public static Element ButtonOrLink(this Element element, string captionOrSelector)
        {
            return element.Button(captionOrSelector, true);
        }

        /// <summary>
        /// Finds a button (button, or input[type=button|reset|submit]) by caption or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="captionOrSelector">caption or css selector</param>
        /// <param name="orLink">if true, links (a elements) are considered buttons too</param>
        /// <returns></returns>
        public static Element Button(this Element element, string captionOrSelector, bool orLink = false)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {
                Element button = null;

                if (FindExt.LooksLikeACssSelector(captionOrSelector))
                {
                    // find by css selector
                    button = element.Query(captionOrSelector).SingleOrDefault(e => (orLink && e.TagName == "a") || e.IsButton());
                    if (button != null)
                    {
                        return button;
                    }
                }

                // find by caption
                string selector = string.Format("input[type=button][value*='{0}'],input[type=submit][value*='{0}'],input[type=reset][value*='{0}'],button:contains('{0}')", captionOrSelector);
                if (orLink)
                {
                    selector += string.Format(",a:contains('{0}')", captionOrSelector);
                }
                button = element.Query(selector).SingleOrDefault();
                if (button == null)
                {
                    // no button found, try to detect bad semantics
                    var elem = element.Query(string.Format(":contains('{0}'):last", captionOrSelector)).SingleOrDefault();
                    if (elem != null)
                    {
                        throw new ElementNotFoundException(string.Format("text '{0}' was found in a {1} element, use input or button elements for buttons (a elements can optionally be used too)", captionOrSelector, elem.TagName));
                    }
                }

                if (button == null)
                {
                    throw new ElementNotFoundException(string.Format("Button{1} '{0}' not found", captionOrSelector, orLink ? " or Link" : ""));
                }
                return button;
            });
        }

        /// <summary>
        /// Finds a fieldset based on its legend or a css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="legendOrSelector">fieldset legend or css selector</param>
        /// <returns></returns>
        public static Element FieldSet(this Element element, string legendOrSelector)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {
                Element fieldset = null;

                if (FindExt.LooksLikeACssSelector(legendOrSelector))
                {
                    // find by css selector
                    fieldset = element.Query(legendOrSelector).SingleOrDefault(e => e.TagName == "fieldset");
                    if (fieldset != null)
                    {
                        return fieldset;
                    }
                }

                // find by legend
                fieldset = element.Query(string.Format("fieldset:contains('{0}')", legendOrSelector))
                    .SingleOrDefault(fs => fs.Query(string.Format("legend:contains('{0}')", legendOrSelector)).Any());

                if (fieldset == null)
                {
                    // no fieldset found, try to detect bad semantics
                    var elem = element.Query(":contains('{0}'):last").SingleOrDefault();
                    if (elem != null)
                    {
                        throw new ElementNotFoundException(string.Format("text '{0}' was found in a {0} element, use a legend element, inside fieldset element, to indentify a FieldSet", legendOrSelector, elem.TagName));
                    }
                }

                if (fieldset == null)
                {
                    throw new ElementNotFoundException(string.Format("FieldSet '{0}' not found", legendOrSelector));
                }
                return fieldset;
            });
        }

    }
}
