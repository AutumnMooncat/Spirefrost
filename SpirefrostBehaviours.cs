using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
