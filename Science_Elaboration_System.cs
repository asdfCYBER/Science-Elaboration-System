using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CommNet;

namespace Science_Elaboration_System
{
    /// <summary>Module for containing data</summary>
    [KSPModule("ModuleDataContainer")]
    public class ModuleDataContainer : PartModule
    {
        [KSPField(isPersistant = true)]
        public int containerSize;

        public Dictionary<string, float> containedData = new Dictionary<string, float>();

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("[SES] OnLoad called");
            Debug.Log(node.ToString());

            base.OnLoad(node);
            containedData.Clear();

            if (node.HasNode("DATACONTAINER"))
            {
                ConfigNode dataNode = node.GetNode("DATACONTAINER");
                int numEntries = dataNode.CountValues / 2;

                for (int i = 1; i <= numEntries; i++)
                {
                    string key = dataNode.GetValue("ID_" + i);
                    float value = float.Parse(dataNode.GetValue("size_" + i));
                    containedData.Add(key, value);
                    Debug.Log("entry " + i + " was added to containedData");
                }
            }

            Debug.Log(DebugDictionary());
            Debug.Log("[SES] OnLoad finished");
        }

        public override void OnSave(ConfigNode node)
        {
            Debug.Log("[SES] OnSave called");
            base.OnSave(node);
            ConfigNode dataNode = new ConfigNode("DATACONTAINER");
            node.AddNode(dataNode);

            Debug.Log(DebugDictionary());

            int index = 1;
            foreach (KeyValuePair<string, float> entry in containedData)
            {
                dataNode.AddValue("ID_" + index, entry.Key);
                dataNode.AddValue("size_" + index, entry.Value);
                index++;
            }
            
            Debug.Log(node.ToString());
            Debug.Log("[SES] OnSave finished");
        }

        public string DebugDictionary()
        {
            string output = "";
            foreach (KeyValuePair<string, float> entry in containedData)
            {
                output += entry.Key + ":" + entry.Value.ToString() + "; \n";
            }
            if (output == "") { return "containedData is empty"; }
            return output;
        }
    }

    /// <summary>Module for catching science experiment results and storing it on the vessel as data</summary>
    [KSPModule("ModuleScienceInterrupter")]
    public class ModuleScienceInterrupter : PartModule
    {
        public void Start()
        {
            GameEvents.OnExperimentStored.Add(ExperimentListener); //replace this later with OnExperimentDeployed and a custom GUI instead as a replacement of the current science report GUI
        }
        public void OnDisable()
        {
            GameEvents.OnExperimentStored.Remove(ExperimentListener);
        }

        public void ExperimentListener(ScienceData scienceData)
        {
            string ID = scienceData.subjectID;
            float size = scienceData.dataAmount;
            Debug.Log("[SES] Experiment catched: {subjectID}");
            StoreData(ID, size);
            //find storage
            //store

            //ScienceSubject scienceSubject = ResearchAndDevelopment.GetSubjectByID(scienceData.subjectID);
            //Debug.Log(ResearchAndDevelopment.GetScienceValue(scienceData.dataAmount, scienceSubject)); //This is the science given when returned
        }

        public void StoreData(string _ID, float _size)
        {
            ModuleDataContainer storageModule = this.part.FindModuleImplementing<ModuleDataContainer>();
            float maxStorage = storageModule.containerSize;
            float usedStorage = storageModule.containedData.Values.Sum();

            if (storageModule.containedData.ContainsKey(_ID))
            {
                Debug.LogWarning("[SES] " + _ID + " is already in the storage module!");
            }
            else if (usedStorage + _size <= maxStorage)
            {
                storageModule.containedData.Add(_ID, _size);
                Debug.Log("[SES] storage used: " + (usedStorage + _size) + " of " + maxStorage);
            }
            else
            {
                Debug.LogWarning("[SES] " + _ID + " does not fit in the storage module: " + usedStorage + "+" + _size + " > " + maxStorage + "!");
            }
        }
    }

    /// <summary>Module for transmitting data</summary>
    [KSPModule("ModuleDataTransmitter")]
    public class ModuleDataTransmitter : PartModule
    {

    }
}
