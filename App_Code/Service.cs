using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService
{
    //Private class attributes
    private XmlDocument xml_Data;//Stores the Xml document.
    private String file_Name;//Stores a valid file/path name for the xml document.
    /*
    Default constructor.
        */
    public Service () {
    }//end Service()

    /*
    This method will return a valid name and path of the youtubeplaylist xml.
        */
    private String fileName()
    {
        return Server.MapPath("App_Data\\youtubeplaylist.xml");
    }//end fileName()

    /*
    Loads the XML data file from a valid filename (path: App_Data\...) into an xml document
    if the xml document is empty, and return a valid xml document. Otherwise, just return an existing
    xml document.
        */
    private XmlDocument xmlData()
    {
        if(xml_Data == null)
        {
            getDataFile();
        }
        return xml_Data;
    }//end getXmlFile()

    private bool getDataFile()
    {
        if (System.IO.File.Exists(fileName()))
        {
            xml_Data = new XmlDocument();
            xml_Data.Load(fileName());
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
    Saves the xml document into the xml file.
        */
    public void saveDataFile()
    {
        if(xml_Data != null)
        {
            xml_Data.Save(fileName());
        }
    }

    /*
  
    */
    private XmlElement getClientList(string nickname)
    {
        XmlElement xmlItems;
        string xPath = "//client[@nickname='" + nickname + "']";
        xmlItems = (XmlElement) xmlData().DocumentElement.SelectSingleNode(xPath);
        return xmlItems;
    }

    private XmlElement getPlayList(string playname)
    {
        XmlElement xmlItems;
        string xPath = "//playlist[@playname='" + playname + "']";
        xmlItems = (XmlElement) xmlData().DocumentElement.SelectSingleNode(xPath);
        return xmlItems;
    }

    private bool insertNewClient(string nickname)
    {
        if(getClientList(nickname) != null)
        {
            return false;
        }
        else
        {
            XmlElement client = xmlData().CreateElement("client");
            XmlAttribute clientAtt = xmlData().CreateAttribute("nickname");
            clientAtt.Value = nickname;
            client.Attributes.Append(clientAtt);
            xmlData().DocumentElement.AppendChild(client);
            return true;
        }
    }

    private XmlElement newElement(string name, string value)
    {
        XmlElement element = xmlData().CreateElement(name);
        element.InnerText = value;
        return element;
    }

    private bool validScore(string score)
    {
        try
        {
            if( Int32.Parse(score) >= 0 && Int32.Parse(score) <= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch(Exception ex)
        {
            return false;
        }
    }

    private string generateGuid()
    {
        return System.Guid.NewGuid().ToString();
    }

    private bool insertNewPlaylist(string nickname, string playlistname)
    {
        try
        {
            XmlElement new_Element;
            XmlElement clientList = getClientList(nickname);
            XmlElement newPlaylist = xmlData().CreateElement("playlist");
            XmlAttribute playlistAtt = xmlData().CreateAttribute("playname");
            playlistAtt.Value = playlistname;
            newPlaylist.Attributes.Append(playlistAtt);
            new_Element = newElement("score", "0");
            newPlaylist.AppendChild(new_Element);
            new_Element = newElement("votecount", "0");
            newPlaylist.AppendChild(new_Element);
            clientList.AppendChild(newPlaylist);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private bool insertNewTrack(string playname, string trackTitle, string urlLocation,
        string duration)
    {
        //try
        //{
            XmlElement new_Element;
            XmlElement playlist = getPlayList(playname);
            XmlElement newTrack = xmlData().CreateElement("track");
            XmlAttribute trackAtt = xmlData().CreateAttribute("id");
            trackAtt.Value = generateGuid();
            newTrack.Attributes.Append(trackAtt);
            new_Element = newElement("title", trackTitle);
            newTrack.AppendChild(new_Element);
            new_Element = newElement("location", urlLocation);
            newTrack.AppendChild(new_Element);
            new_Element = newElement("duration", duration);
            newTrack.AppendChild(new_Element);
            playlist.AppendChild(newTrack);
            return true;
        //}
        //catch(Exception ex)
        //{
        //    return false;
        //}
    }

    /*
    Start of web methods ******************************************************************
    */

    [System.Web.Services.WebMethod]
    /*
    Gets the current element and all of its child nodes.
        */
    public string getClientPlaylistCollection(string nickname)
    {
        return getClientList(nickname).OuterXml;
    }

    [System.Web.Services.WebMethod]
    public string getPlaylist(string nickname, string playname)
    {
        XmlElement playlists = getClientList(nickname);
        XmlElement playlist = (System.Xml.XmlElement) playlists.SelectSingleNode("//playlist[@playname='" + playname + "']");
        if(playlist != null)
        {
            return playlist.OuterXml;
        }
        else
        {
            return "</playlist>";
        }
    }

    [System.Web.Services.WebMethod]
    public bool createNewClient(string nickname)
    {
        if (insertNewClient(nickname))
        {
            saveDataFile();
            return true;
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool createNewPlayList(string nickname, string playlistname)
    {
        if (insertNewPlaylist(nickname, playlistname))
        {
            saveDataFile();
            return true;
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool createNewTrack(string playname, string trackTitle, string urlLocation,
        string duration)
    {
        if (insertNewTrack(playname, trackTitle, urlLocation, duration))
        {
            saveDataFile();
            return true;
        }
        else
        {
            return false;
        }
    }

}//end class