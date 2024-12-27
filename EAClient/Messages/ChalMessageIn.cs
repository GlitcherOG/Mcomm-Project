﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSX3_Server.EAClient.Messages
{
    public class ChalMessageIn : EAMessage
    {
        public override string MessageType { get { return "chal"; } }

        public string PERS;

        public override void AssignValues()
        {
            if (stringDatas.Count > 0)
            {
                PERS = stringDatas[0].Value;
            }
        }

        public override void AssignValuesToString()
        {
            AddStringData("PERS", PERS);
        }
    }
}
