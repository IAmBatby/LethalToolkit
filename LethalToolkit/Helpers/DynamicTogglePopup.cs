using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalToolkit
{
    public class DynamicTogglePopup
    {
        public Dictionary<string, bool> toggleOptions;
        public string[] ToggleOptions => toggleOptions.Keys.ToArray();

        public string CurrentSelection { get; internal set; } = string.Empty;
        public int CurrentSelectionIndex => ToggleOptions.ToList().IndexOf(CurrentSelection);
        public string PreviousSelection { get; internal set; } = string.Empty;
        public int PreviousSelectionIndex => ToggleOptions.ToList().IndexOf(PreviousSelection);


        public DynamicTogglePopup(string[] newToggleOptions)
        {
            toggleOptions = new Dictionary<string, bool>();
            foreach (string newToggleOption in newToggleOptions)
                toggleOptions.Add(newToggleOption, false);
            CurrentSelection = ToggleOptions[0];
        }

        public void Toggle(string newSelection)
        {
            if (newSelection != CurrentSelection)
            {
                Debug.Log("Toggle: " + newSelection + ToggleOptions.ToList().IndexOf(newSelection));
                toggleOptions[newSelection] = true;
            }
            PreviousSelection = CurrentSelection;
            CurrentSelection = newSelection;
        }

        public void Toggle(int index)
        {
            if (index > -1 && ToggleOptions.Length > index)
                Toggle(ToggleOptions[index]);
        }

        public bool CheckToggle(int index)
        {
            if (index > 0 && ToggleOptions.Length > index)
                return (CheckToggle(ToggleOptions[index]));
            else
                return (false);
        }

        public bool CheckToggle(string toggleOption)
        {
            bool returnBool = toggleOptions[toggleOption];

            if (returnBool == true)
                toggleOptions[toggleOption] = false;

            return (returnBool);
        }

        public void Clear()
        {
            foreach (string toggleOption in ToggleOptions)
                toggleOptions[toggleOption] = false;
        }
    }
}
