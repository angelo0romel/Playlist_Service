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
    private static bool validUser = false;
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

    private XmlDocument xmlUser()
    {
        XmlDocument xml_User = new XmlDocument();
        xml_User.Load(Server.MapPath("App_Data\\authenticate.xml"));
        return xml_User;
    }//end getXmlFile()

    private XmlElement getUserCredentials(string username)
    {
        XmlElement validUser;
        string xPath = "//user[@name='" + username + "']";
        validUser = (XmlElement) xmlUser().DocumentElement.SelectSingleNode(xPath);
        return validUser;
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

    private XmlElement getTrackList(string id)
    {
        XmlElement xmlItems;
        string xPath = "//track[@id='" + id + "']";
        xmlItems = (XmlElement)xmlData().DocumentElement.SelectSingleNode(xPath);
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

    private bool isValidScore(string score)
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
        catch
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
        catch
        {
            return false;
        }
    }

    private bool insertNewTrack(string playname, string trackTitle, string urlLocation,
        string duration)
    {
        try
        {
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
        }
        catch
        {
            return false;
        }
    }

    /*
    Start of web methods ******************************************************************
    */
    [System.Web.Services.WebMethod]
    public bool login(string username, string password)
    {   
        try {
            XmlElement user = getUserCredentials(username);
            XmlNode userNode = user.SelectSingleNode("//user[@name='" + username + "']");
            if (userNode["password"].InnerText.Trim() == password)
            {
                validUser = true;
                return true;
            }
            else
            {
                validUser = false;
                return false;
            }
        }
        catch
        {
            return false;
        }        
    }

    [System.Web.Services.WebMethod]
    public void logout()
    {
        validUser = false;
    }

    [System.Web.Services.WebMethod]
    /*
    Gets the current element and all of its child nodes.
        */
    public string getClientPlaylistCollection(string nickname)
    {
        if (validUser)
        {
            return getClientList(nickname).OuterXml;
        }
        else
        {
            return "";
        }
    }

    [System.Web.Services.WebMethod]
    public string getPlaylist(string nickname, string playname)
    {
        if (validUser)
        {
            XmlElement playlists = getClientList(nickname);
            XmlElement playlist = (System.Xml.XmlElement)playlists.SelectSingleNode("//playlist[@playname='" + playname + "']");
            if (playlist != null)
            {
                return playlist.OuterXml;
            }
            else
            {
                return "</playlist>";
            }
        }
        else
        {
            return "";
        }
    }

    [System.Web.Services.WebMethod]
    public bool createNewClient(string nickname)
    {
        if (validUser)
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
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool createNewPlayList(string nickname, string playlistname)
    {
        if (validUser)
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
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool createNewTrack(string playname, string trackTitle, string urlLocation,
        string duration)
    {
        if (validUser)
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
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool updateClientName(string nickname, string newClientNickName)
    {
        if (validUser)
        {
            try
            {
                XmlElement clients = getClientList(nickname);
                XmlElement client = (System.Xml.XmlElement)clients.SelectSingleNode("//client[@nickname='" + nickname + "']");
                client.Attributes[0].Value = newClientNickName;
                saveDataFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool updatePlaylistName(string nickname, string playname, string newPlayname)
    {
        if (validUser)
        {
            try
            {
                XmlElement playlists = getClientList(nickname);
                XmlElement playlist = (System.Xml.XmlElement)playlists.SelectSingleNode("//playlist[@playname='" + playname + "']");
                playlist.Attributes[0].Value = newPlayname;
                saveDataFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public string getTrackInfo(string playname, string trackID)
    {
        if (validUser)
        {
            try
            {
                XmlElement playlist = getPlayList(playname);
                XmlElement trackList = (System.Xml.XmlElement)playlist.SelectSingleNode("//track[@id='" + trackID + "']");
                return trackList.OuterXml;
            }
            catch
            {
                return "</track>";
            }
        }
        else
        {
            return "";
        }
    }

    [System.Web.Services.WebMethod]
    public bool updateTrack(string playname, string trackID, string title, string location, string duration)
    {
        if (validUser)
        {
            try
            {
                XmlElement playlist = getPlayList(playname);
                XmlElement trackList = (System.Xml.XmlElement)playlist.SelectSingleNode("//track[@id='" + trackID + "']");
                trackList["title"].InnerText = title;
                trackList["location"].InnerText = location;
                trackList["duration"].InnerText = duration;
                saveDataFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool removeTrack(string playname, string trackID)
    {
        if (validUser)
        {
            try
            {
                XmlElement playlist = getPlayList(playname);
                XmlNode trackList = (System.Xml.XmlElement)playlist.SelectSingleNode("//track[@id='" + trackID + "']");
                trackList.ParentNode.RemoveChild(trackList);
                saveDataFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool removePlaylist(string playname)
    {
        if (validUser)
        {
            try
            {
                XmlElement playlist = getPlayList(playname);
                XmlNode playlistNode = playlist.SelectSingleNode("//playlist[@playname='" + playname + "']");
                playlistNode.ParentNode.RemoveChild(playlistNode);
                saveDataFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool removeClient(string nickname)
    {
        if (validUser)
        {
            try
            {
                XmlElement client = getClientList(nickname);
                XmlNode clientNode = client.SelectSingleNode("//client[@nickname='" + nickname + "']");
                clientNode.ParentNode.RemoveChild(clientNode);
                saveDataFile();
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [System.Web.Services.WebMethod]
    public bool voteOnPlaylist(string playname, double score)
    {
        if (validUser)
        {
            try
            {
                if (isValidScore(score.ToString()))
                {
                    XmlElement playlist = getPlayList(playname);
                    XmlNode playlistNode = playlist.SelectSingleNode("//playlist[@playname='" + playname + "']");

                    playlistNode["votecount"].InnerText = (Convert.ToInt32(playlistNode["votecount"].InnerText.Trim()) + 1).ToString();

                    if (Convert.ToInt32(playlistNode["votecount"].InnerText.Trim()) < 1)
                    {
                        playlistNode["score"].InnerText = score.ToString();
                    }
                    else
                    {
                        playlistNode["score"].InnerText = ((score +
                         Convert.ToDouble(playlistNode["score"].InnerText.Trim())) /
                         Convert.ToInt32(playlistNode["votecount"].InnerText.Trim())).ToString();
                    }

                    saveDataFile();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

}//end class