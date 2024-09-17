using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SaveData
{
    public SaveData(int money)
    {
        this.money = money;
    }

    public int money;
}

public class SaveManager
{
    
    public void Save(int myMoney)
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream fs = null;
        if (!File.Exists(Application.persistentDataPath + "/saveData.txt"))
        {
            fs = File.Create(Application.persistentDataPath + "/saveData.txt");
        }
        else
        {
            fs = File.Open(Application.persistentDataPath + "/saveData.txt", FileMode.OpenOrCreate);
        }

        SaveData sd = new SaveData(myMoney);
        bf.Serialize(fs, sd);
        fs.Close();
    }

    public SaveData Load()
    {
        SaveData data = null;

        if(File.Exists(Application.persistentDataPath + "/saveData.txt"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(Application.persistentDataPath + "/saveData.txt", FileMode.OpenOrCreate);
            data = (SaveData)bf.Deserialize(fs);
            fs.Close();
        }

        if(data == null)
        {
            data = new SaveData(0);
        }
        return data;
    }
}
