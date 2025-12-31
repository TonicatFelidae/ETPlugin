using ET.FileSystem.FileReader;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class FileManagerDemo : MonoBehaviour
    {
        [SerializeField] FileReaderManager _fileManager;
        public string protocolID;

        public void Start()
        {
            List<Dictionary<string,object>> dat = _fileManager.GetData(protocolID);
            Debug.Log(dat.Count);
            for (int i = 0; i < dat.Count; i++) 
            {
                Debug.Log($" {dat[i].Count}");
                if (dat[i].Count>0)
                {
                    Debug.Log($" {((List<string>)(dat[i].FirstOrDefault().Value)).Count}");

                }
            }
            Debug.Log(dat[0].ToString());
            //List<string> ss = (List<string>)(dat[0]["0"]);
             //_fileManager.ReadData<string>(ref readDat0, ET.FileSystem.DataExtractorProtocolID.ConversationCsv);
        }
    }
}
