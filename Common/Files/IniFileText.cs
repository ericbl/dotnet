﻿#region info
/* 
 * Ini Class - Version 1.0
 * Copyright (C) 2015 Petros Kyladitis <http://www.multipetros.gr/>
 * 
 * Complete INI file properties manipulation class. 
 * 
 * This is free software, distributed under the therms and conditions of the FreeBSD License. For the full License text
 * see at the 'license.txt' file, distributed with this project or at <http://www.multipetros.gr/freebsd-license/>
 */
#endregion

using System;
using System.Collections.Generic;

namespace Common.Files
{
    /// <summary>
    /// Manage an .ini file as text file, i.e. parsing it.
    /// </summary>
    public class IniFileText
    {

        private Dictionary<string, Dictionary<string, string>> sections;  //store all file structure 
                                                                          //with sections as keys and nested lines as values
        private char[] separator = new char[1];        //1-cell char array to store separator
        private string separatorStr;                   //separator as string
        private char[] comments = new char[1];         //1-cell char array to store comments start sign
        private string commentsStr;                    //comments sign as string
        private string file;                           //properties file path
        private bool autosave;                         //autosave mode option
        private bool ignoreCase;                       //ignore case filter for keys option
        private bool collectComments;                  //collect the comments in the sections

        /// <summary>
        /// The simplest constructor, with filename only to configure. Autosave properties setted as False, ingore case as True, the Separator as "=" and comments sign as "#".
        /// </summary>
        /// <param name="file">A valid file path, with read and write priviliges</param>
        public IniFileText(string file) 
            : this(file, false)
        {
        }

        /// <summary>
        /// Constructor with filename and autosave mode to configure. Ingore case setted as True, the Separator as "=" and Comments sign as "#".
        /// </summary>
        /// <param name="file">A valid file path, with read and write priviliges</param>
        /// <param name="autosave">True, to save each time a Property added or setted. False, to save manualy by calling the Save() method</param>
        public IniFileText(string file, bool autosave) : this(file, autosave, true) { }

        /// <summary>
        /// Constructor with filename, autosave mode and ignore case to configure. Separator setted as "=" and Comments sign as "#".
        /// </summary>
        /// <param name="file">A valid file path, with read and write priviliges</param>
        /// <param name="autosave">True, to save each time a Property added or setted. False, to save manualy by calling the Save() method</param>
        /// <param name="ignoreCase">True to ignore case, False to make properties keys case sensitive</param>
        public IniFileText(string file, bool autosave, bool ignoreCase) : this(file, autosave, ignoreCase, '=') { }

        /// <summary>
        /// Constructor with filename, autosave mode, ignore case and the separator sign to configure. The Comments sign setted as "#".
        /// </summary>
        /// <param name="file">A valid file path, with read and write priviliges</param>
        /// <param name="autosave">True, to save each time a Property added or setted. False, to save manualy by calling the Save() method</param>
        /// <param name="ignoreCase">True to ignore case, False to make properties keys case sensitive</param>
        /// <param name="separator">Separator character for keys and values</param>
        public IniFileText(string file, bool autosave, bool ignoreCase, char separator) : this(file, autosave, ignoreCase, separator, '#') { }


        /// <summary>
        /// Initializes a new instance of the <see cref="IniFileText"/> class.
        /// Constructor with filename, autosave mode, ignore case, the separator and comments sign to configure.
        /// </summary>
        /// <param name="file">A valid file path, with read and write priviliges</param>
        /// <param name="autosave">True, to save each time a Property added or setted. False, to save manualy by calling the Save() method</param>
        /// <param name="ignoreCase">True to ignore case, False to make properties keys case sensitive</param>
        /// <param name="separator">Separator character for keys and values</param>
        /// <param name="comments">Comments definition character</param>
        /// <param name="collectComments">if set to <c>true</c> collect the comments; otherwise ignore them.</param>
        public IniFileText(string file, bool autosave = false, bool ignoreCase = true, char separator = '=', char comments = '#', bool collectComments = true)
        {
            this.file = file;
            this.autosave = autosave;
            this.ignoreCase = ignoreCase;
            this.separator[0] = separator;
            this.separatorStr = separator.ToString();
            this.comments[0] = comments;
            this.commentsStr = comments.ToString();
            this.collectComments = collectComments;
            Init();
        }

        /// <summary>
        /// Initialize the object by loading sections and properties and their values and comments from specified file
        /// </summary>
        private void Init()
        {
            //initialize dictionary to store the INI file structure, with sections as keys 
            //and other lines (properties and line comments) as values in nested dictionaries
            sections = new Dictionary<string, Dictionary<string, string>>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            if (System.IO.File.Exists(file))
            {                     //if file not exist, don't do anything else
                string[] lines = null;                //init a new string array to store the file lines
                try
                {
                    lines = System.IO.File.ReadAllLines(file);  //load lines from file, on error throws an exception
                }
                catch (Exception) { }
                string sectionName = string.Empty;    //default section name is an empty string
                int sectionStart;                     //section name start position
                int sectionEnd;                       //section name end position
                string[] keyval;                      //2-cell array to store key and value of each line
                                                      //initilize dictionary to store the 1st founded section properties, setting the ignore case as the selected
                var properties = CreateDictionary();
                int i = 0;                            //line number counter
                foreach (string line in lines)
                {         //analyze line by line					
                    if (line.Contains("[") && line.Contains("]") && !line.StartsWith(commentsStr))
                    {    //if the chars '[' and ']' a section founded, so extract its name included in these chars


                        if (properties.Count > 0)
                        {                    //if initialized properties dictionary has values, means that a previous existed so,
                            sections[sectionName] = properties;     //add the founded properties to the sections dictionary, with se previous section name
                        }
                        sectionStart = line.IndexOf("[") + 1;       //find the start index of the section name
                        sectionEnd = line.IndexOf("]") - 1;         //find the last index of the section name
                        if (sectionStart > sectionEnd)
                        {               //if ending bracket founded before starting, eg(']abc[')
                            continue;                               //ignore & go to next line
                        }

                        sectionName = line.Substring(sectionStart, sectionEnd); //get the secton name, who is inside the brackets. Any other characters just ignored.
                                                                                //initialize a new properties dictionary, to store the properties of the founded section
                        properties = CreateDictionary();
                    }
                    else
                    {  //this line is property or standalone comment. Analyze and extract it.
                        if (line.Contains(separatorStr))
                        {            //if no separator exist, this is not a property line
                            keyval = line.Split(separator, 2);     //split line at the 1st separator appearacne
                            keyval[0] = keyval[0].Trim();          //trim property's name from spaces
                            if (properties.ContainsKey(keyval[0]))
                            {  //if key name already exist, continue to the next line
                                continue;                          //no duplicate key names allowed, in this case kept the 1st that appeared
                            }
                            keyval[1] = keyval[1].Trim();          //trim property's value line from spaces
                            properties[keyval[0]] = keyval[1];     //put this part of the line, as value to properties dictionary of the current section
                        }
                        else if (line.Contains(commentsStr) && collectComments)
                        {       //if there's no separator char, but only a comments sign
                                //this is a standalone comment line. Keep the comment, wich begins at the index of the comment sign
                                //until the end of the line. Any preceding characters are trimmed off. The comment will saved to the
                                //current section properties dictionary, with the comment as value, and the comment sign combined with
                                //the current line number as key (eg. #15=xxx). This, keep the key name unique.
                            properties[commentsStr + i.ToString()] = line.Substring(line.IndexOf(comments[0]));
                        }
                    }
                    i++;  //increase the line counter
                }
                //store the latest founded set of properties to the last founded section
                if (properties.Count > 0)
                {
                    sections[sectionName] = properties;
                }
            }
        }

        private Dictionary<string, string> CreateDictionary()
        {
            return new Dictionary<string, string>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);            
        }

        /// <summary>
        /// Store properties to file
        /// </summary>
        public void Save()
        {
            string output = string.Empty; //initilize output string
                                //add the default section (empty name) properties line by line
            if (sections.ContainsKey(string.Empty))
            {
                foreach (KeyValuePair<string, string> mainKeyVal in sections[string.Empty])
                {
                    if (mainKeyVal.Key.StartsWith(commentsStr))
                    {
                        //if theres is a standalone comment line add comment sign and the stored value
                        output += commentsStr + mainKeyVal.Value;
                    }
                    else
                    {
                        output += mainKeyVal.Key + separatorStr + mainKeyVal.Value;
                    }
                    output += "\r\n";
                }
                output += "\r\n";
            }
            //foreach named sections, add the section name and the properties to the output line by line
            foreach (KeyValuePair<string, Dictionary<string, string>> sectsKeyval in sections)
            {
                if (sectsKeyval.Key != string.Empty)
                {
                    //add section name, encloded in brackets
                    output += "[" + sectsKeyval.Key + "]\r\n";
                    //add the properties of the current section
                    foreach (KeyValuePair<string, string> propsKeyval in sectsKeyval.Value)
                    {
                        if (propsKeyval.Key.StartsWith(commentsStr))
                        {
                            //if theres is a standalone comment line add comment sign and the stored value
                            output += propsKeyval.Value;
                        }
                        else
                        {
                            output += propsKeyval.Key + separatorStr + propsKeyval.Value;

                        }
                        output += "\r\n";
                    }
                    output += "\r\n";
                }
            }
            System.IO.File.WriteAllText(file, output);
        }

        /// <summary>
        /// Strip value part from comments
        /// </summary>
        /// <param name="val">Value text</param>
        /// <returns>The value striped from comments and trimmed from spaces between value and comments</returns>
        private string StripComments(string val)
        {
            if (val.IndexOf(comments[0]) < 0)
            {
                return val;
            }
            else
            {
                return val.Substring(0, val.IndexOf(comments[0])).Trim();
            }
        }

        /// <summary>
        /// Store a new value to the value part of the line, keeping the existed comments
        /// </summary>
        /// <param name="currentVal">Current value part of the line (with value and comments)</param>
        /// <param name="newVal">The new value</param>
        /// <returns>The new value part of the line</returns>
        private string StoreKeepingComments(string currentVal, string newVal)
        {
            return newVal + currentVal.Substring(StripComments(currentVal).Length);
        }

        /// <summary>
        /// Access properties like Dictionary, negotiating with the default (noname section) with the Property key name only eg: iniObj["keyName"].
        /// When getting value of the specified property, or empty string if property not found
        /// When setting a value for a non-existed property, the property will be automatically added.
        /// If the setting value is <em>null</em> the property will be deleted.
        /// </summary>
        public string this[string key]
        {
            get
            {
                return this[string.Empty, key];
            }
            set
            {
                this[string.Empty, key] = value;
            }
        }

        /// <summary>
        /// Access properties like Dictionary, negotiating with the Section and the Property key name eg: iniObj["sectionName", "keyName"]
        /// When getting value of the specified section-property pair, or empty string if property not found
        /// When setting a value for a non-existed section-property pair, the section-property pair will be automatically added.
        /// If the setting value is <em>null</em> the property will be deleted.
        /// </summary>
        public string this[string section, string key]
        {
            get
            {
                if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
                {
                    return StripComments(sections[section][key]);
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (value != null)
                {
                    if (!sections.ContainsKey(section))
                    {
                        sections[section] = CreateDictionary(); // new Dictionary<string, string>();
                    }
                    if (sections[section].ContainsKey(key))
                    {
                        sections[section][key] = StoreKeepingComments(sections[section][key], value);
                    }
                    else
                    {
                        sections[section][key] = value;
                    }
                }
                else
                {
                    //if setting value is null and key exist, remove this key
                    if (sections.ContainsKey(section) && sections[section].ContainsKey(key))
                    {
                        sections[section].Remove(key);
                    }
                }
                if (autosave)
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Enum section settings keys.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>
        /// A list of.
        /// </returns>
        public ICollection<string> EnumSectionSettingsKeys(string section)
        {
            return EnumSectionSettings(section)?.Keys;
        }

        /// <summary>
        /// Enum section settings values.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>
        /// A list of.
        /// </returns>
        public ICollection<string> EnumSectionSettingsValues(string section)
        {
            return EnumSectionSettings(section)?.Values;
        }

        /// <summary>
        /// Enum the section settings.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns>
        /// A Dictionary&lt;string,string&gt;
        /// </returns>
        public Dictionary<string, string> EnumSectionSettings(string section)
        {
            if (sections.ContainsKey(section))
            {
                return sections[section];
            }
            else
            {
                return null;
            }
        }
    }
}
