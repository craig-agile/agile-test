using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech;
using System.Speech.Recognition;

namespace The_Echo_Environment
{
    class Echo
    {
        /*
               These are the internal structures that are manipulated within the <c>Echo</c> class.
          
               Using these internal structures, I was able to hold any data pertaining to a particular voice recognition
               set of grammar with its appropriate response.
        
         */
        #region Echo's Internal Structures
        /**
         * <summary> 
         *      The <c>Key_and_Command</c> structure holds the an index to a given ignition word/phrase and a list
         *      of indexs referencing a key to set of choices (via the <c>Key_and_Choice</c> structure).
         * </summary>
         * <remarks>
         *      Using the <c>Key_and_Command</c> structure is a keen way of linking a ignition word/phrases to a set of
         *      choices (if at all). To refrence the ignition word (for simple games where saying the object name will
         *      suffice), simply either not link the ignition word to a set of choices of remove all the choices from that
         *      ignition word upon the event that question is asked.
         * </remarks>
         * */
        internal struct Key_and_Command
        {
            /// <summary>
            /// Is an idex to a specified key word or phrase.
            /// </summary>
            public ushort Key;

            /// <summary>
            /// Is a <c>List</c> of unsigned shorts that references a set of choices within a
            /// <c>Key_and_Choice</c> structure. <b>Leave null if the word itself causes an action.</b>
            /// </summary>
            public List<ushort> Command_Set;
        }

        /**
         * <summary>
         *      The <c>Key_and_Choice</c> structure links a given index to an array of strings which hold the values
         *      of your choices.
         * </summary>
         * <remarks>
         *     Using the <c>Key_and_Choice</c> helps segment a given set of choices with the use of an index (the <c>Key</c>).
         *     Once this structure has been initialized, you can simply refrence through the index number and get a set of
         *     choices at any given time (or until the index is removed).
         * </remarks>
         * */
        internal struct Key_and_Choice
        {
            /// <summary>
            /// Holds the index for a given set of choices.
            /// </summary>
            public ushort Key;

            /// <summary>
            /// Holds an array of choices (in the form of a <c>string</c> array).
            /// </summary>
            public string[] Choice_Set;
        }

        /**
         * <summary>
         *      The <c>Working_Set</c> holds all the current data pertaining to what the voice recognition engine is using
         *      or about to use in regards to keywords/phrases and choices.
         * </summary>
         * <remarks>
         *      The <c>Working_Set</c> structure holds a list of keywords and phrases that are currently in use or waiting
         *      to be used shortly in the <c>List</c> variable <c>Ignition_Set</c>.  The <c>List</c> variable <c>Builders_IU</c>
         *      (standing for <c>GrammarBUILDER</c>S In Use) holds the <c>GrammarBuilder</c> representation of the
         *      <c>Ignition_Set</c>. Once we have all that we need, we choose to consolidate our <c>Builders_IU</c> into a
         *      set of choices. Lastly, the <c>Grammar_IU</c>(for GRAMMAR In Use) is given the consolidated choices which will
         *      then be used in the recognition engine.
         *      
         *      This may seem like alot of steps, but it breaks everything down evenly.
         * </remarks>
         * */
        internal struct Working_Set
        {
            /// <summary>
            /// Holds a set of indexes that reference back to the <c>Key_and_Command</c> structure within the <c>Echo</c> class.
            /// </summary>
            public List<ushort> Inginition_Set;

            /// <summary>
            /// Holds a <c>List</c> of <c>GrammarBuilder</c> objects, that are representations of what is refrenced from within
            /// the <c>Ignition_Set</c>.
            /// </summary>
            public List<GrammarBuilder> Builders_IU;

            /// <summary>
            /// The <c>Choice_Combinaion</c> holds all the <c>GrammarBuilder</c> objects supplied by the <c>Builders_IU</c> 
            /// varibale, in the form of a set of Choices. This can then be turned into a signle grammar.
            /// </summary>
            public Choices Choice_Combination;

            /// <summary>
            /// The <c>Grammar_IU</c> holds all the Grammars that were created by the <c>Choice_Combination</c> variable. We can
            /// then submit this to the recognition engine during the recognition process.
            /// </summary>
            public Grammar Grammar_IU;
        }
        #endregion

        #region The Echo Class' Data Memebers
        /// <summary>
        /// Holds a <c>List</c> of <c>Key_and_Command</c> structures to keep track of any ignition words or phrases that were
        /// added to the <c>Echo</c> class.
        /// </summary>
        private List<Key_and_Command> Chain_Base;
        /// <summary>
        /// Holds a <c>List</c> of <c>GrammarBuilder</c> objects that holds the ignition words or pharses in the appropriate
        /// format for the <c>SpeechRcognitionEngine</c>.
        /// </summary>
        private List<GrammarBuilder> Ignition_Phords;
        
        /// <summary>
        /// Holds a <c>List</c> of <c>Key_and_Choice</c> structures that keep track of any choice sets that have been created
        /// and linked to a respective keyword or phrase.
        /// </summary>
        private List<Key_and_Choice> Choice_Dictionary;
        /// <summary>
        /// Holds a <c>List</c> of <c>Choices</c> objects that holds a given set of choices in a format that can be easily linked
        /// to a keyword or phrase upon the developer's descretion.
        /// </summary>
        private List<Choices> Choice_Base;

        /// <summary>
        /// Holds any data that is currently being recognized or will be recognized by the <c>SpeechRecognitionEngie</c> object.
        /// </summary>
        private Working_Set Recognition_Table;

        /// <summary>
        /// This object triggers events necessary to react to the user upon a given keyword or phrase that is either by itself
        /// or link with a set of choices.
        /// </summary>
        private SpeechRecognitionEngine Recogn_Engine;

        /// <summary>
        /// The confidence level in which the <c>SpeechRecognitionEngine</c>'s certainty of a recognized speech pattern must
        /// either be equal to or greater than in order to initialize any sort of reaction.
        /// </summary>
        private float Confidence_Level;

        /// <summary>
        /// The mode in which the <c>SpeechRecognitionEngine</c> object is currently operating at.
        /// </summary>
        private RecognizeMode Recognition_Type;
        #endregion

        /**
         * <summary>
         *     The <c>Current_Confidence</c> property gives you the current confidence level a given keyword or phrase it must
         *     pass before any actions are performed.
         * </summary>
         * <remarks>
         *      The <c>Current_Confidence</c> property can be used to alter the <c>Confidence_Level</c> within the <c>Echo</c> class at anytime you choose.
         *      So, in the event that you either need to raise, lower, or simply check the current confidence level during run-time, you can use this
         *      property.
         * </remarks>
         * <value>
         *     The Current_Confidence property gets/sets the <c>Echo</c> class' <c>Confidence_Level</c> member. 
         * </value>
         * <returns>
         *      Returns the current confidence level that the recognition engine is using currently.
         * </returns>
         * */
        public float Current_Confidence
        {
            get
            {
                return this.Confidence_Level;
            }

            set
            {
                this.Confidence_Level = (float)value <= 1 && (float)value > 0 ? (float)value : 0.0f;
            }
        }

        /**
         * <summary>
         *      The <c>Echo</c> constructor is used to initialize the starting confidence level and the starting recognition type.
         * </summary>
         * <remarks>
         *      The <c>Echo</c> constructor will instantiate all of its private data members and assign the initial confidence level and recognition
         *      mode the recognition engine is in. You can the two following modes <c>RecognizeMode.Multiple</c> - where the speech recognition object
         *      listens until told to stop - or <c>RecognizeMode.Single</c> - where once a speech pattern is recognized, it stops running. <em>Needless to
         *      say, the <c>RecognizedMode.Multiple</c> is the preferred choice.</em>
         * </remarks>
         * <param name="Initial_Confidence">
         *      The initial confidence level that a given speech pattern must make before a reaction is made by the
         *      recognition engine.
         * </param>
         * <param name="Initial_Recognition">
         *      The initial recognition mode the recognition engine will start off with.
         * </param>
         * <example>
         *      <b>An example of initializing the constuctor is the following:</b>
         *      
         *          <code>
         *                  Echo_Major = new Echo(.80f, RecognizeMode.Multiple);
         *          </code>
         *          
         *      This will set thw initial confidence level at eighty percent (80%) and sets the recognition engine to recognize multiple speech patterns.
         * </example>
         * */
        public Echo(float Initial_Confidence, RecognizeMode Initial_Recognition)
        {
            this.Confidence_Level = Initial_Confidence;
            this.Recognition_Type = Initial_Recognition;

            this.Recogn_Engine = new SpeechRecognitionEngine();
            this.Recogn_Engine.SetInputToDefaultAudioDevice();

            #region
            
            EventHandler<SpeechRecognizedEventArgs>[] My_Events = new EventHandler<SpeechRecognizedEventArgs>[4];
            My_Events[0] = new EventHandler<SpeechRecognizedEventArgs>(Default_Recognition);
            My_Events[1] = new EventHandler<SpeechRecognizedEventArgs>(Default_Recognition_II);
           // My_Events[1] = new EventHandler<SpeechRecognizedEventArgs>((sender, e) => this.Default_Recognition_III(sender, e, "repairs", "What exactly needs repairing?", this.Ham));
            My_Events[2] = new EventHandler<SpeechRecognizedEventArgs>((sender, e) => this.Default_Recognition_III(sender, e, "shoot", "Why do you want to shoot?", this.Eggs));
            My_Events[3] = new EventHandler<SpeechRecognizedEventArgs>(doSomething);


            this.Recogn_Engine.SpeechRecognized += My_Events[0];
            //this.Recogn_Engine.SpeechRecognized += My_Events[1];
            //this.Recogn_Engine.SpeechRecognized += My_Events[2];
            
            #endregion

            this.Chain_Base = new List<Key_and_Command>();
            this.Choice_Base = new List<Choices>();
            this.Choice_Dictionary = new List<Key_and_Choice>();
            this.Ignition_Phords = new List<GrammarBuilder>();

            this.Recognition_Table.Builders_IU = new List<GrammarBuilder>();
            this.Recognition_Table.Choice_Combination = new Choices();
            this.Recognition_Table.Inginition_Set = new List<ushort>();
        }

        private void Default_Recognition(object sender, SpeechRecognizedEventArgs e)
        {
            if(this.Confidence_Level <= e.Result.Confidence)
                Console.Out.WriteLine(e.Result.Text);
        }

        private void Default_Recognition_II(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.Confidence_Level <= e.Result.Confidence)
                Console.Out.WriteLine("I recognized something...");
        }

        private void Default_Recognition_III(object sender, SpeechRecognizedEventArgs e, string Saying, string Output, Action My_Activity)
        {
            if (this.Confidence_Level <= e.Result.Confidence)
                if (e.Result.Text.Contains(Saying))
                {
                    Console.Out.WriteLine(Output);
                    My_Activity.BeginInvoke(null, this);
                }
        }

        private void doSomething(object sender, SpeechRecognizedEventArgs e)
        {
            Console.Out.WriteLine("Something was done");
        }
        private void Ham()
        {
            Console.Out.WriteLine("Ham");
        }

        private void Eggs()
        {
            Console.Out.WriteLine("Eggs");
        }

        /**
         * <summary>
         *      The <c>Add_Choices</c> method lets you add a set of choices to the <c>Echo</c> class' internal choice
         *      dictionary.
         * </summary>
         * <remarks>
         * 
         * </remarks>
         * <param name="Set_Choices">
         *      Takes a string array of choices and places them within the Echo's <c>Choice_Dictionary</c>.
         * </param>
         * 
         * <example>
         *          <para>
         *              <b>An example of creating a set of choices would be the following:</b>
         *              <para>
         *                  <code>
         *                          Echo_Major.Add_Choices(new string[] {"foo", "bar"});
         *                  </code>
         *              </para>
         *          </para>
         *</example>
         * */
        public void Add_Choices(string[] Set_Choices)
        {
            int Index = this.Choice_Dictionary.Count;
            Key_and_Choice New_ChoiceSet;

            New_ChoiceSet.Key = (ushort)Index;
            New_ChoiceSet.Choice_Set = Set_Choices;

            this.Choice_Dictionary.Add(New_ChoiceSet);

            this.Choice_Base.Add(new Choices(this.Choice_Dictionary[Index].Choice_Set));
        }

        /**
         * <summary>
         *      The <c>Add_Ignition_Word</c> method allows the developer to add a new ignition word or phrase to the <c>Echo</c>
         *      class' internal data member <c>Ignition_Phords</c>.
         * </summary>
         * <remarks>
         *      The <c>Add_Ignition_Word</c> method lets the developer add a new ignition word or phrase (<em>think of a keyword
         *      </em>) to the <c>Echo</c> class' internal data member <c>Ignition_Phords</c>. Once this word or phrase is added,
         *      you can either:
         *      <ul>
         *              <li>
         *                  Add the ignition word or phrase to the <c>Working_Set</c> via the <c>Add_Ignition_to_WS</c>
         *                  method. This allows the word or pharse alone to initiate a select action once called
         *                  (assuming the voice recognition engine in on).
         *              </li>
         *              <li>
         *                  Link the ignition word or phrase to a set of choices via the <c>Combine_ItoC</c> method.
         *                  This will bind the ignition word or phrase to each choice within the selected choice set.
         *              </li>
         *      </ul>
         * </remarks>
         * <param name="I_Word">
         *      The <c>I_Word</c> is the ignition word or phrase you want to add to the <c>Echo</c> class.
         * </param>
         * <example>
         *      <b>An example of using the <c>Add_Ignition_Word</c> method is the following:</b>
         *      <code>
         *              Echo_Major.Add_Ignition_Word("Choo");
         *              Echo_Major.Add_Ignition_Word("Dominick");
         *      </code>
         *      
         *      This will add the ignition words <em>Choo</em> and <em>Dominick</em> to the <c>Echo</c> class'
         *      <c>Ignition_Phords</c> variable.
         * </example>
         * */
        public void Add_Ignition_Word(string I_Word)
        {
            int Index = (this.Ignition_Phords != null) ? this.Ignition_Phords.Count : 0;
            this.Ignition_Phords.Add(new GrammarBuilder(I_Word));

            Key_and_Command New_KaC;
            New_KaC.Key = (ushort)Index;
            New_KaC.Command_Set = new List<ushort>();

            this.Chain_Base.Add(New_KaC);
        }

        public void Add_Ignition_To_WS(ushort Index)
        {
            if (this.Chain_Base.Count >= Index)
                this.Recognition_Table.Builders_IU.Add(this.Ignition_Phords[(int)Index]);
        }

        private void Consolidate_Choices()
        {
            if (this.Recognition_Table.Builders_IU.Count != 0)
                this.Recognition_Table.Choice_Combination = new Choices(this.Recognition_Table.Builders_IU.ToArray());
        }

        private void Consolidate_Grammar()
        {
            if (this.Recognition_Table.Choice_Combination != null)
                this.Recognition_Table.Grammar_IU = new Grammar((GrammarBuilder)this.Recognition_Table.Choice_Combination);
        }

        /**
         * <exception cref="System.IndexOutOfRangeException">
         *      This exception is thrown when the either index given to the <c>Combine_ItoC</c> Method is not within either
         *      in the <c>Ignition_Phords</c> member's or the <c>Choice_Base</c> member's index range. 
         * </exception>
         * */
        public void Combine_ItoC(int Index_I, int Index_C)
        {
            if (this.Ignition_Phords.Count <= Index_I + 1 && this.Choice_Base.Count <= Index_C + 1)
            {
                this.Chain_Base[Index_I].Command_Set.Add((ushort)Index_C);
                this.Ignition_Phords[Index_I].Append(this.Choice_Base[Index_C]);
            }
            else
            {
                throw new System.IndexOutOfRangeException("The index you selected is out of range!");
            }
        }

        private void Load_Recog_Engine()
        {
            if (this.Recognition_Table.Grammar_IU != null)
            {
                this.Recogn_Engine.LoadGrammar(this.Recognition_Table.Grammar_IU);
                this.Recogn_Engine.RequestRecognizerUpdate();
            }
        }

        public void Reconsolidate()
        {
            this.Consolidate_Choices();
            this.Consolidate_Grammar();
            this.Load_Recog_Engine();
        }

        public void Start_Recognition()
        {
            this.Recogn_Engine.RecognizeAsync(this.Recognition_Type);
        }
    }
}

//Below is the entire Echo class again as it was with the Choo Environment. It worked but I created a simplified
//version for now that I am using in the class, thus it is here for safekeeping.

/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech;
using System.Speech.Recognition;

namespace ChooSingleForm
{
    class Echo
    {
         /*
               These are the internal structures that are manipulated within the <c>Echo</c> class.
          
               Using these internal structures, I was able to hold any data pertaining to a particular voice recognition
               set of grammar with its appropriate response.
        
         */
        #region Echo's Internal Structures
        /**
         * <summary> 
         *      The <c>Key_and_Command</c> structure holds the an index to a given ignition word/phrase and a list
         *      of indexs referencing a key to set of choices (via the <c>Key_and_Choice</c> structure).
         * </summary>
         * <remarks>
         *      Using the <c>Key_and_Command</c> structure is a keen way of linking a ignition word/phrases to a set of
         *      choices (if at all). To refrence the ignition word (for simple games where saying the object name will
         *      suffice), simply either not link the ignition word to a set of choices of remove all the choices from that
         *      ignition word upon the event that question is asked.
         * </remarks>
         * */
        internal struct Key_and_Command
        {
            /// <summary>
            /// Is an idex to a specified key word or phrase.
            /// </summary>
            public ushort Key;

            /// <summary>
            /// Is a <c>List</c> of unsigned shorts that references a set of choices within a
            /// <c>Key_and_Choice</c> structure. <b>Leave null if the word itself causes an action.</b>
            /// </summary>
            public List<ushort> Command_Set;
        }

        /**
         * <summary>
         *      The <c>Key_and_Choice</c> structure links a given index to an array of strings which hold the values
         *      of your choices.
         * </summary>
         * <remarks>
         *     Using the <c>Key_and_Choice</c> helps segment a given set of choices with the use of an index (the <c>Key</c>).
         *     Once this structure has been initialized, you can simply refrence through the index number and get a set of
         *     choices at any given time (or until the index is removed).
         * </remarks>
         * */
        internal struct Key_and_Choice
        {
            /// <summary>
            /// Holds the index for a given set of choices.
            /// </summary>
            public ushort Key;

            /// <summary>
            /// Holds an array of choices (in the form of a <c>string</c> array).
            /// </summary>
            public string[] Choice_Set;
        }

        /**
         * <summary>
         *      The <c>Working_Set</c> holds all the current data pertaining to what the voice recognition engine is using
         *      or about to use in regards to keywords/phrases and choices.
         * </summary>
         * <remarks>
         *      The <c>Working_Set</c> structure holds a list of keywords and phrases that are currently in use or waiting
         *      to be used shortly in the <c>List</c> variable <c>Ignition_Set</c>.  The <c>List</c> variable <c>Builders_IU</c>
         *      (standing for <c>GrammarBUILDER</c>S In Use) holds the <c>GrammarBuilder</c> representation of the
         *      <c>Ignition_Set</c>. Once we have all that we need, we choose to consolidate our <c>Builders_IU</c> into a
         *      set of choices. Lastly, the <c>Grammar_IU</c>(for GRAMMAR In Use) is given the consolidated choices which will
         *      then be used in the recognition engine.
         *      
         *      This may seem like alot of steps, but it breaks everything down evenly.
         * </remarks>
         * */
        internal struct Working_Set
        {
            /// <summary>
            /// Holds a set of indexes that reference back to the <c>Key_and_Command</c> structure within the <c>Echo</c> class.
            /// </summary>
            public List<ushort> Inginition_Set;

            /// <summary>
            /// Holds a <c>List</c> of <c>GrammarBuilder</c> objects, that are representations of what is refrenced from within
            /// the <c>Ignition_Set</c>.
            /// </summary>
            public List<GrammarBuilder> Builders_IU;

            /// <summary>
            /// The <c>Choice_Combinaion</c> holds all the <c>GrammarBuilder</c> objects supplied by the <c>Builders_IU</c> 
            /// varibale, in the form of a set of Choices. This can then be turned into a signle grammar.
            /// </summary>
            public Choices Choice_Combination;

            /// <summary>
            /// The <c>Grammar_IU</c> holds all the Grammars that were created by the <c>Choice_Combination</c> variable. We can
            /// then submit this to the recognition engine during the recognition process.
            /// </summary>
            public Grammar Grammar_IU;
        }
        #endregion

        #region The Echo Class' Data Memebers
        /// <summary>
        /// Holds a <c>List</c> of <c>Key_and_Command</c> structures to keep track of any ignition words or phrases that were
        /// added to the <c>Echo</c> class.
        /// </summary>
        private List<Key_and_Command> Chain_Base;
        /// <summary>
        /// Holds a <c>List</c> of <c>GrammarBuilder</c> objects that holds the ignition words or pharses in the appropriate
        /// format for the <c>SpeechRcognitionEngine</c>.
        /// </summary>
        private List<GrammarBuilder> Ignition_Phords;
        
        /// <summary>
        /// Holds a <c>List</c> of <c>Key_and_Choice</c> structures that keep track of any choice sets that have been created
        /// and linked to a respective keyword or phrase.
        /// </summary>
        private List<Key_and_Choice> Choice_Dictionary;
        /// <summary>
        /// Holds a <c>List</c> of <c>Choices</c> objects that holds a given set of choices in a format that can be easily linked
        /// to a keyword or phrase upon the developer's descretion.
        /// </summary>
        private List<Choices> Choice_Base;

        /// <summary>
        /// Holds any data that is currently being recognized or will be recognized by the <c>SpeechRecognitionEngie</c> object.
        /// </summary>
        private Working_Set Recognition_Table;

        /// <summary>
        /// This object triggers events necessary to react to the user upon a given keyword or phrase that is either by itself
        /// or link with a set of choices.
        /// </summary>
        private SpeechRecognitionEngine Recogn_Engine;

        /// <summary>
        /// The confidence level in which the <c>SpeechRecognitionEngine</c>'s certainty of a recognized speech pattern must
        /// either be equal to or greater than in order to initialize any sort of reaction.
        /// </summary>
        private float Confidence_Level;

        /// <summary>
        /// The mode in which the <c>SpeechRecognitionEngine</c> object is currently operating at.
        /// </summary>
        private RecognizeMode Recognition_Type;
        #endregion

        /**
         * <summary>
         *     The <c>Current_Confidence</c> property gives you the current confidence level a given keyword or phrase it must
         *     pass before any actions are performed.
         * </summary>
         * <remarks>
         *      The <c>Current_Confidence</c> property can be used to alter the <c>Confidence_Level</c> within the <c>Echo</c> class at anytime you choose.
         *      So, in the event that you either need to raise, lower, or simply check the current confidence level during run-time, you can use this
         *      property.
         * </remarks>
         * <value>
         *     The Current_Confidence property gets/sets the <c>Echo</c> class' <c>Confidence_Level</c> member. 
         * </value>
         * <returns>
         *      Returns the current confidence level that the recognition engine is using currently.
         * </returns>
         * */
        public float Current_Confidence
        {
            get
            {
                return this.Confidence_Level;
            }

            set
            {
                this.Confidence_Level = (float)value <= 1 && (float)value > 0 ? (float)value : 0.0f;
            }
        }

        /**
         * <summary>
         *      The <c>Echo</c> constructor is used to initialize the starting confidence level and the starting recognition type.
         * </summary>
         * <remarks>
         *      The <c>Echo</c> constructor will instantiate all of its private data members and assign the initial confidence level and recognition
         *      mode the recognition engine is in. You can the two following modes <c>RecognizeMode.Multiple</c> - where the speech recognition object
         *      listens until told to stop - or <c>RecognizeMode.Single</c> - where once a speech pattern is recognized, it stops running. <em>Needless to
         *      say, the <c>RecognizedMode.Multiple</c> is the preferred choice.</em>
         * </remarks>
         * <param name="Initial_Confidence">
         *      The initial confidence level that a given speech pattern must make before a reaction is made by the
         *      recognition engine.
         * </param>
         * <param name="Initial_Recognition">
         *      The initial recognition mode the recognition engine will start off with.
         * </param>
         * <example>
         *      <b>An example of initializing the constuctor is the following:</b>
         *      
         *          <code>
         *                  Echo_Major = new Echo(.80f, RecognizeMode.Multiple);
         *          </code>
         *          
         *      This will set thw initial confidence level at eighty percent (80%) and sets the recognition engine to recognize multiple speech patterns.
         * </example>
         * */

        private EventHandler<SpeechRecognizedEventArgs>[] My_Events;

        public Echo(float Initial_Confidence, RecognizeMode Initial_Recognition)
        {
            this.Confidence_Level = Initial_Confidence;
            this.Recognition_Type = Initial_Recognition;

            this.Recogn_Engine = new SpeechRecognitionEngine();
            this.Recogn_Engine.SetInputToDefaultAudioDevice();

            #region

            My_Events = new EventHandler<SpeechRecognizedEventArgs>[3];
           
            /*EventHandler<SpeechRecognizedEventArgs>[] My_Events = new EventHandler<SpeechRecognizedEventArgs>[3];
            My_Events[0] = new EventHandler<SpeechRecognizedEventArgs>(Default_Recognition);
            My_Events[1] = new EventHandler<SpeechRecognizedEventArgs>((sender, e) => this.Default_Recognition_III(sender, e, "repairs", "What exactly needs repairing?", this.Ham));
            My_Events[2] = new EventHandler<SpeechRecognizedEventArgs>((sender, e) => this.Default_Recognition_III(sender, e, "shoot", "Why do you want to shoot?", this.Eggs));

            this.Recogn_Engine.SpeechRecognized += My_Events[0];
            this.Recogn_Engine.SpeechRecognized += My_Events[1];
            this.Recogn_Engine.SpeechRecognized += My_Events[2];*/
            
            #endregion

            this.Chain_Base = new List<Key_and_Command>();
            this.Choice_Base = new List<Choices>();
            this.Choice_Dictionary = new List<Key_and_Choice>();
            this.Ignition_Phords = new List<GrammarBuilder>();

            this.Recognition_Table.Builders_IU = new List<GrammarBuilder>();
            this.Recognition_Table.Choice_Combination = new Choices();
            this.Recognition_Table.Inginition_Set = new List<ushort>();
        }


       

        public void clear()
        {
            //Clear all events by setting each value to null
            for (int i = 0; i < My_Events.Length; i++)
            {
                My_Events[i] = null;
            }
        }


       

         public void addEventsII(string word, Action triggerMethod)
        {
            for (int i = 0; i < My_Events.Length; i++)
            {
                //Add the method to the last element of the list of events
                if (My_Events[i] == null)
                {
                    My_Events[i] = new EventHandler<SpeechRecognizedEventArgs>((sender, e) => this.Default_Recognition_IV(sender, e, word, triggerMethod));
                    this.Recogn_Engine.SpeechRecognized += My_Events[i];
                   
                    //MessBox.ShowBox(i.ToString());
                    break;
                }
            }
         }

        //This method takes another method as a parameter which will be called in the
        //event that speech is recognized
        public void addEvents(EventHandler<SpeechRecognizedEventArgs> myMethod)
        {
            
            for (int i = 0; i < My_Events.Length; i++)
            {
                //Add the method to the last element of the list of events
                if (My_Events[i] == null)
                {
                    My_Events[i] = myMethod;
                    this.Recogn_Engine.SpeechRecognized += My_Events[i];
                    //MessBox.ShowBox(i.ToString());
                    break;
                }
            }
        }

        private void Default_Recognition(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.Confidence_Level <= e.Result.Confidence)
                 Console.Out.WriteLine(e.Result.Text);
            
        }

        private void Default_Recognition_II(object sender, SpeechRecognizedEventArgs e)
        {
            if (this.Confidence_Level <= e.Result.Confidence)
                Console.Out.WriteLine("I recognized something...");
        }


        private void Default_Recognition_IV(object sender, SpeechRecognizedEventArgs e, string Saying, Action My_Activity)
        {
            if (this.Confidence_Level <= e.Result.Confidence)
                if (e.Result.Text.Contains(Saying))
                {
                   //Invoke the appropriate activity
                   My_Activity.Invoke();
                    
                }
        }

        /**
         * <summary>
         *      The <c>Add_Choices</c> method lets you add a set of choices to the <c>Echo</c> class' internal choice
         *      dictionary.
         * </summary>
         * <remarks>
         * 
         * </remarks>
         * <param name="Set_Choices">
         *      Takes a string array of choices and places them within the Echo's <c>Choice_Dictionary</c>.
         * </param>
         * 
         * <example>
         *          <para>
         *              <b>An example of creating a set of choices would be the following:</b>
         *              <para>
         *                  <code>
         *                          Echo_Major.Add_Choices(new string[] {"foo", "bar"});
         *                  </code>
         *              </para>
         *          </para>
         *</example>
         * */
        public void Add_Choices(string[] Set_Choices)
        {
            int Index = this.Choice_Dictionary.Count;
            Key_and_Choice New_ChoiceSet;

            New_ChoiceSet.Key = (ushort)Index;
            New_ChoiceSet.Choice_Set = Set_Choices;

            this.Choice_Dictionary.Add(New_ChoiceSet);

            this.Choice_Base.Add(new Choices(this.Choice_Dictionary[Index].Choice_Set));
        }

        /**
         * <summary>
         *      The <c>Add_Ignition_Word</c> method allows the developer to add a new ignition word or phrase to the <c>Echo</c>
         *      class' internal data member <c>Ignition_Phords</c>.
         * </summary>
         * <remarks>
         *      The <c>Add_Ignition_Word</c> method lets the developer add a new ignition word or phrase (<em>think of a keyword
         *      </em>) to the <c>Echo</c> class' internal data member <c>Ignition_Phords</c>. Once this word or phrase is added,
         *      you can either:
         *      <ul>
         *              <li>
         *                  Add the ignition word or phrase to the <c>Working_Set</c> via the <c>Add_Ignition_to_WS</c>
         *                  method. This allows the word or pharse alone to initiate a select action once called
         *                  (assuming the voice recognition engine in on).
         *              </li>
         *              <li>
         *                  Link the ignition word or phrase to a set of choices via the <c>Combine_ItoC</c> method.
         *                  This will bind the ignition word or phrase to each choice within the selected choice set.
         *              </li>
         *      </ul>
         * </remarks>
         * <param name="I_Word">
         *      The <c>I_Word</c> is the ignition word or phrase you want to add to the <c>Echo</c> class.
         * </param>
         * <example>
         *      <b>An example of using the <c>Add_Ignition_Word</c> method is the following:</b>
         *      <code>
         *              Echo_Major.Add_Ignition_Word("Choo");
         *              Echo_Major.Add_Ignition_Word("Dominick");
         *      </code>
         *      
         *      This will add the ignition words <em>Choo</em> and <em>Dominick</em> to the <c>Echo</c> class'
         *      <c>Ignition_Phords</c> variable.
         * </example>
         * */
        public void Add_Ignition_Word(string I_Word)
        {
            int Index = (this.Ignition_Phords != null) ? this.Ignition_Phords.Count : 0;
            this.Ignition_Phords.Add(new GrammarBuilder(I_Word));

            Key_and_Command New_KaC;
            New_KaC.Key = (ushort)Index;
            New_KaC.Command_Set = new List<ushort>();

            this.Chain_Base.Add(New_KaC);
        }

        public void Add_Ignition_To_WS(ushort Index)
        {
            if (this.Chain_Base.Count >= Index)
                this.Recognition_Table.Builders_IU.Add(this.Ignition_Phords[(int)Index]);
        }

        private void Consolidate_Choices()
        {
            if (this.Recognition_Table.Builders_IU.Count != 0)
                this.Recognition_Table.Choice_Combination = new Choices(this.Recognition_Table.Builders_IU.ToArray());
        }

        private void Consolidate_Grammar()
        {
            if (this.Recognition_Table.Choice_Combination != null)
                this.Recognition_Table.Grammar_IU = new Grammar((GrammarBuilder)this.Recognition_Table.Choice_Combination);
        }

        /**
         * <exception cref="System.IndexOutOfRangeException">
         *      This exception is thrown when the either index given to the <c>Combine_ItoC</c> Method is not within either
         *      in the <c>Ignition_Phords</c> member's or the <c>Choice_Base</c> member's index range. 
         * </exception>
         * */
        public void Combine_ItoC(int Index_I, int Index_C)
        {
           // MessBox.ShowBox(Ignition_Phords[Index_I].ToString());
            if (this.Ignition_Phords.Count <= Index_I + 1 && this.Choice_Base.Count <= Index_C + 1)
            {
                this.Chain_Base[Index_I].Command_Set.Add((ushort)Index_C);
                this.Ignition_Phords[Index_I].Append(this.Choice_Base[Index_C]);
            }
            else
            {
                throw new System.IndexOutOfRangeException("The index you selected is out of range!");
            }
        }

        private void Load_Recog_Engine()
        {
            if (this.Recognition_Table.Grammar_IU != null)
            {
                this.Recogn_Engine.LoadGrammar(this.Recognition_Table.Grammar_IU);
                this.Recogn_Engine.RequestRecognizerUpdate();
            }
        }

        public void Reconsolidate()
        {
            this.Consolidate_Choices();
            this.Consolidate_Grammar();
            this.Load_Recog_Engine();
        }

        public void Start_Recognition()
        {
            this.Recogn_Engine.RecognizeAsync(this.Recognition_Type);
        }
    
    }
}
*/