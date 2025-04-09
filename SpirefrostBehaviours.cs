using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Spirefrost
{
    internal class UpdateManager : MonoBehaviour
    {
        public void Update()
        {
            MainModFile.instance.updated = false;
        }
    }
}
