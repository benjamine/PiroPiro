using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro
{
    /// <summary>
    /// Extension methods to work with input fields on html forms
    /// </summary>
    public static class FormInputExt
    {
        /// <summary>
        /// Clear element content and send keys (valid for text input elements)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="keys"></param>
        private static void ClearAndSendKeys(this Element element, string keys)
        {
            element.Clear();
            element.SendKeys(keys);
        }

        /// <summary>
        /// Element is an input button (input[type=button|submit|reset] or button)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsButton(this Element element)
        {
            string type = element.InputType;
            if (type == "button" || type == "reset" || type == "submit")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Element is an hyperlink (a, anchor tag)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsLink(this Element element)
        {
            string type = element.InputType;
            if (type == "link")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Element is an input field
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsField(this Element element)
        {
            string type = element.InputType;
            if (string.IsNullOrEmpty(type) || type == "button" || type == "reset" || type == "submit")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Element is input[type=text] or textarea
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsTextField(this Element element)
        {
            string type = element.InputType;
            return type == "text" || type == "textarea";
        }

        /// <summary>
        /// Element is input[type=select] or select
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsSelect(this Element element)
        {
            return element.InputType == "select";
        }

        /// <summary>
        /// Is this element checked? (if this is a checkbox)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsChecked(this Element element)
        {
            if (element.InputType != "checkbox")
            {
                throw new WrongElementTypeException("only a checkbox input element can be checked or unchecked", element);
            }
            return (element.GetAttribute("checked") ?? "").Trim().ToLower() == "checked";
        }

        /// <summary>
        /// Checks this element (if this is a checkbox)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static void Check(this Element element)
        {
            if (!element.IsChecked())
            {
                element.Click();
            }
        }

        /// <summary>
        /// Unchecks this element (if this is a checkbox)
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static void Uncheck(this Element element)
        {
            if (element.IsChecked())
            {
                element.Click();
            }
        }

        /// <summary>
        /// Fill this element with a value, valid for input fields
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static void FillWith(this Element field, string value)
        {
            string tagName = field.TagName;
            string type = field.InputType;
            if (field.IsTextField())
            {
                if (field.Classes.Contains("ckeditor"))
                {
                    string id = field.GetAttribute("id");
                    string val = value.Replace("\\", "\\\\").Replace("\'", "\\'").Replace("\n", "\\n").Replace("\r", "\\r");
                    // support for filling CKEditor wysiwyg html editor
                    field.Browser.ExecuteJS(@"(function(id,val) { 
                        if (typeof CKEDITOR == 'undefined' || !CKEDITOR.instances[id]) { 
                            document.getElementById(id).value = val;
                        } else { 
                            CKEDITOR.instances[id].insertText(val); 
                        }})('" + id + "','" + val + "');");
                }
                else
                {
                    field.ClearAndSendKeys(value);
                }
            }
            else if (type == "checkbox")
            {
                bool check;
                switch (value.Trim().ToLower())
                {
                    case "1":
                    case "ok":
                    case "yes":
                    case "true":
                        check = true;
                        break;
                    case "0":
                    case "no":
                    case "false":
                        check = false;
                        break;
                    default:
                        throw new Exception(string.Format("Invalid value for checkbox: '{0}', try 'ok', 'yes', 'no', 'true', 'false', '1', '0'.", value));
                }
                if (check)
                {
                    field.Check();
                }
                else
                {
                    field.Uncheck();
                }
            }
            else if (type == "select")
            {
                field.SelectOptionByText(value);
            }
            else if (type == "file")
            {
                field.SetFile(value);
            }
            else
            {
                throw new WrongElementTypeException(string.Format("Cannot fill \"{0}[type={1}]\", unsupported element", field.TagName, field.GetAttribute("type")), field);
            }
        }

        /// <summary>
        /// Fills a set of input fields using a dictionary of values
        /// </summary>
        /// <param name="element"></param>
        /// <param name="fieldValues"></param>
        public static void FillFields(this Element element, IDictionary<string, string> fieldValues)
        {
            foreach (var fieldValue in fieldValues)
            {
                element.Field(fieldValue.Key).FillWith(fieldValue.Value);
            }
        }

        /// <summary>
        /// Gets the value of an input field
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string GetFieldValue(this Element field)
        {
            if (!field.IsField())
            {
                throw new WrongElementTypeException("cannot get value from a non field element", field);
            }

            string tag = field.TagName;
            string type = field.InputType;
            if (tag == "textarea")
            {
                return field.Text;
            }
            else if (type == "text")
            {
                return field.GetAttribute("value");
            }
            else if (type == "checkbox")
            {
                return field.IsChecked().ToString();
            }
            else if (type == "select")
            {
                return field.SelectedOptionsText();
            }
            else
            {
                throw new WrongElementTypeException(string.Format("Cannot read field value \"{0}[type={1}]\", unsupported element", field.TagName, field.GetAttribute("type")), field);
            }
        }
    }
}
