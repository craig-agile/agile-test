using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech;
using System.Speech.Recognition;
using System.Media;
using System.Reflection;
using System.Reflection.Emit;

namespace The_Echo_Environment
{
    class Program
    {
        static Echo Echo_Major;

        static void Main(string[] args)
        {
            Echo_Major = new Echo(.85f, RecognizeMode.Multiple);
            Echo_Major.Add_Ignition_Word("Test");//Corresponds to element 0 in the working set
          //  Echo_Major.Add_Ignition_Word("Granny");//Corresponds to element 1 in the working set
          //  Echo_Major.Add_Choices(new string[] {"shoot target", "get repairs", "talk to a Transformer"});
          //  Echo_Major.Combine_ItoC(0, 0);
            Echo_Major.Add_Ignition_To_WS(0);
           // Echo_Major.Add_Ignition_To_WS(3);

            Echo_Major.Reconsolidate();

            Echo_Major.Start_Recognition();
            Echo_Major.Current_Confidence = .80F;
            float Noony = Echo_Major.Current_Confidence;

            
            while (true) ;

            
        }
    }
}
