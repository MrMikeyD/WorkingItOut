using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;

namespace WorkingItOut
{
    public class WorkoutListView : ListView, IClearable, IDetach
    {
        #region Constructors

        public WorkoutListView()
        {
            this.workouts = new ObservableCollection<Workout>();

            // DEBUG: auto fill workout list
            this.LoadDefaultWorkout();

            this.ItemsSource = this.workouts;
        }

        #endregion Constructors

        #region Events

        #endregion End of Events

        #region Methods

        /// <summary>
        /// Clears this control, returning it to a neutral state
        /// </summary>
        public void Clear()
        {
            foreach (Workout workout in this.workouts)
            {
                workout.Detach();
            }

            this.workouts.Clear();
        }

        /// <summary>
        /// Detachs this control, readying it for disposal
        /// </summary>
        public void Detach()
        {
            this.Clear();
        }

        private void LoadDefaultWorkout()
        {
            this.workouts.Clear();

            Workout workout = new Workout();

            workout.BodyWeight = 113.9d;
            workout.StartDate = new DateTime(2015, 1, 2);
            workout.EndDate = new DateTime(2015, 1, 8);
            ObservableCollection<Exercise> exercises = new ObservableCollection<Exercise>();

            foreach(ExerciseType exerciseType in Enum.GetValues(typeof(ExerciseType)))
            {
                Exercise exercise = new Exercise();
                exercise.ExerciseType = exerciseType;
                exercise.Weight = 15;
                exercise.Reps = new ObservableCollection<int>() { 8,8,10 };
                
                exercises.Add(exercise);
            }

            workout.Exercises = exercises;
        }

        /// <summary>
        /// Loads the workout file at the given filepath
        /// </summary>
        public Boolean LoadWorkoutFile(String filepath)
        {
            Boolean loaded = false;

            if (!String.IsNullOrEmpty(filepath))
            {
                List<Workout> workouts = new List<Workout>();

                using (Stream stream = new FileStream(filepath,
                                            FileMode.Open,
                                            FileAccess.Read,
                                            FileShare.Read))
                {

                    try
                    {
                        IFormatter formatter = new BinaryFormatter();
                        workouts = (List<Workout>)formatter.Deserialize(stream);
                        loaded = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deserializing workout file: " + ex.Message);
                    }
                    finally
                    {
                        stream.Close();
                    }
                }

                this.Clear();

                if (workouts != null && workouts.Count > 0)
                {
                    foreach (Workout workout in workouts)
                    {
                        this.workouts.Add(workout);
                    }
                }
            }

            return loaded;
        }

        #endregion End of Methods

        #region Properties

        #endregion End of Properties

        #region Members

        private ObservableCollection<Workout> workouts;

        #endregion End of Members

        #region Private Classes

        /// <summary>
        /// Data class representing a full week's workout 
        /// </summary>
        [Serializable]
        private class Workout : Object, ISerializable, INotifyPropertyChanged, IClearable, IDetach
        {

            #region Constructors

            public Workout()
            {

            }

            public Workout(SerializationInfo info, StreamingContext context)
            {
                this.bodyWeight = info.GetDouble(Workout.BodyWeightPropertyName.ToLower());
                this.startDate = info.GetDateTime(Workout.StartDatePropertyName.ToLower());
                this.endDate = info.GetDateTime(Workout.EndDatePropertyName.ToLower());
                this.numExercises = info.GetInt32(Workout.NumExercisesPropertyName.ToLower());
                this.exercises = new ObservableCollection<Exercise>();

                // Deserialize all exercises
                for (int i = 0; i < this.numExercises; i++)
                {
                    this.exercises.Add((Exercise)info.GetValue(Workout.ExercisesPropertyName.ToLower() + String.Format("_{0}", i),
                        typeof(Exercise)));
                }

                this.exercises.CollectionChanged += this.OnExercisesChanged;
            }

            #endregion Constructors

            #region Events

            /// <summary>
            /// Handles the event triggered when the exercises have changed
            /// </summary>
            private void OnExercisesChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    this.NotifyPropertyChanged(Workout.NumExercisesPropertyName);
                }

                this.NotifyPropertyChanged(Workout.ExercisesPropertyName);
            }

            #endregion End of Events

            #region Methods

            /// <summary>
            /// Clears this object, returning it to a neutral state
            /// </summary>
            public void Clear()
            {
                if (this.exercises != null)
                {
                    foreach (Exercise exercise in this.exercises)
                    {
                        exercise.Clear();
                    }

                    this.exercises.Clear();
                }
            }

            /// <summary>
            /// Detachs this object, readying it for disposal
            /// </summary>
            public void Detach()
            {
                foreach (Exercise exercise in this.exercises)
                {
                    exercise.Detach();
                }

                if (this.exercises != null)
                {
                    this.exercises.CollectionChanged -= this.OnExercisesChanged;
                    this.exercises.Clear();
                }
            }

            /// <summary>
            /// Serializes the workout entry object
            /// </summary>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(Workout.BodyWeightPropertyName.ToLower(), this.bodyWeight);
                info.AddValue(Workout.StartDatePropertyName.ToLower(), this.startDate);
                info.AddValue(Workout.EndDatePropertyName.ToLower(), this.endDate);
                info.AddValue(Workout.NumExercisesPropertyName.ToLower(), this.numExercises);

                // Serialize all exercises with array index tacked on
                for (int i = 0; i < this.numExercises; i++)
                {
                    info.AddValue(Workout.ExercisesPropertyName.ToLower() + String.Format("_{0}", i),
                        this.exercises[i]);
                }
            }

            /// <summary>
            /// Sends out an event notifying anyone that cares that the given property
            /// has changed
            /// </summary>
            private void NotifyPropertyChanged(String propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion End of Methods

            #region Properties

            /// <summary>
            /// Gets or sets the recorded body weight at the end of this week
            /// </summary>
            public Double BodyWeight
            {
                get { return this.bodyWeight; }
                set
                {
                    if (this.bodyWeight != value)
                    {
                        this.bodyWeight = value;

                        this.NotifyPropertyChanged(Workout.BodyWeightPropertyName);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the Start Date
            /// </summary>
            public DateTime StartDate
            {
                get { return this.startDate; }
                set
                {
                    if (this.startDate != value)
                    {
                        this.startDate = value;

                        this.NotifyPropertyChanged(Workout.StartDatePropertyName);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the End Date
            /// </summary>
            public DateTime EndDate
            {
                get { return this.endDate; }
                set
                {
                    if (this.endDate != value)
                    {
                        this.endDate = value;

                        this.NotifyPropertyChanged(Workout.EndDatePropertyName);
                    }
                }
            }

            /// <summary>
            /// Gets the number of exercises
            /// </summary>
            public Int32 NumExercises
            {
                get { return this.exercises != null ? this.exercises.Count : 0; }
            }

            /// <summary>
            /// Gets or sets the Exercises 
            /// </summary>
            public ObservableCollection<Exercise> Exercises
            {
                get { return this.exercises; }
                set
                {
                    if (this.exercises != value)
                    {
                        if (this.exercises != null)
                        {
                            this.exercises.CollectionChanged -= this.OnExercisesChanged;
                        }

                        this.exercises = value;

                        if (this.exercises != null)
                        {
                            this.exercises.CollectionChanged += this.OnExercisesChanged;
                        }

                        this.NotifyPropertyChanged(Workout.NumExercisesPropertyName);
                        this.NotifyPropertyChanged(Workout.ExercisesPropertyName);
                    }
                }
            }

            #endregion End of Properties

            #region Members

            private Double bodyWeight;
            private DateTime startDate;
            private DateTime endDate;
            private Int32 numExercises;
            private ObservableCollection<Exercise> exercises;

            public event PropertyChangedEventHandler PropertyChanged;
            public const String BodyWeightPropertyName = "BodyWeight";
            public const String StartDatePropertyName = "StartDate";
            public const String EndDatePropertyName = " EndDate";
            public const String NumExercisesPropertyName = "NumExercises";
            public const String ExercisesPropertyName = "Exercises";

            #endregion End of Members
        }

        /// <summary>
        /// Data class representing a single exercise
        /// </summary>
        [Serializable]
        private class Exercise : Object, ISerializable, INotifyPropertyChanged, IClearable, IDetach
        {

            #region Constructors

            public Exercise()
            {

            }

            /// <summary>
            /// Constructs a workout object from a serialized context
            /// </summary>
            public Exercise(SerializationInfo info, StreamingContext context)
            {
                Int32 sets;

                this.exerciseType = (ExerciseType)info.GetInt32(Exercise.ExerciseTypePropertyName.ToLower());
                this.weight = info.GetInt32(Exercise.WeightPropertyName.ToLower());
                sets = info.GetInt32(Exercise.SetsPropertyName.ToLower());
                this.reps = new ObservableCollection<Int32>();

                // Deserialize all reps
                for (int i = 0; i < sets; i++)
                {
                    this.reps.Add(info.GetInt32(Exercise.RepsPropertyName.ToLower() + String.Format("_{0}", i)));
                }

                this.reps.CollectionChanged += this.OnRepsChanged;
            }

            #endregion Constructors

            #region Events

            /// <summary>
            /// Handles the event triggered when the reps have changed
            /// </summary>
            private void OnRepsChanged(Object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                this.NotifyPropertyChanged(Exercise.RepsPropertyName);

                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                    e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    this.NotifyPropertyChanged(Exercise.SetsPropertyName);
                }
            }

            #endregion End of Events

            #region Methods

            /// <summary>
            /// Clears this object, returning it to a neutral state
            /// </summary>
            public void Clear()
            {
                if (this.reps != null)
                {
                    this.reps.Clear();
                }
            }

            /// <summary>
            /// Detachs this object, readying it for disposal
            /// </summary>
            public void Detach()
            {
                if (this.reps != null)
                {
                    this.reps.CollectionChanged -= this.OnRepsChanged;
                }

                this.Clear();
            }

            /// <summary>
            /// Serializes the workout object
            /// </summary>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(Exercise.ExerciseTypePropertyName.ToLower(), (Int32)this.exerciseType);
                info.AddValue(Exercise.WeightPropertyName.ToLower(), this.weight);
                info.AddValue(Exercise.SetsPropertyName.ToLower(), this.Sets);

                // Serialize all reps with array index tacked on
                for (int i = 0; i < this.Sets; i++)
                {
                    info.AddValue(Exercise.RepsPropertyName.ToLower() + String.Format("_{0}", i),
                        this.reps[i]);
                }
            }

            /// <summary>
            /// Sends out an event notifying anyone that cares that the given property
            /// has changed
            /// </summary>
            private void NotifyPropertyChanged(String propertyName)
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion End of Methods

            #region Properties

            /// <summary>
            /// Gets or sets the Exercise Type
            /// </summary>
            public ExerciseType ExerciseType
            {
                get { return this.exerciseType; }
                set
                {
                    if (this.exerciseType != value)
                    {
                        this.exerciseType = value;

                        this.NotifyPropertyChanged(Exercise.ExerciseTypePropertyName);
                    }
                }
            }

            /// <summary>
            /// Gets or sets the weight (in pounds)
            /// </summary>
            public Int32 Weight
            {
                get { return this.weight; }
                set
                {
                    if (this.weight != value)
                    {
                        this.weight = value;

                        this.NotifyPropertyChanged(Exercise.WeightPropertyName);
                    }
                }

            }

            /// <summary>
            /// Gets or sets the repeitions
            /// </summary>
            public ObservableCollection<Int32> Reps
            {
                get { return this.reps; }
                set
                {
                    if (this.reps != value)
                    {
                        if (this.reps != null)
                        {
                            this.reps.CollectionChanged -= this.OnRepsChanged;
                        }

                        this.reps = value;

                        if (this.reps != null)
                        {
                            this.reps.CollectionChanged += this.OnRepsChanged;
                        }

                        this.NotifyPropertyChanged(Exercise.RepsPropertyName);
                    }
                }

            }

            /// <summary>
            /// Gets or sets the number of sets
            /// </summary>
            public Int32 Sets
            {
                get { return this.reps != null ? this.reps.Count : 0; }
            }

            #endregion End of Properties

            #region Members

            private ExerciseType exerciseType;
            private Int32 weight;
            private ObservableCollection<Int32> reps;

            public event PropertyChangedEventHandler PropertyChanged;
            public const String ExerciseTypePropertyName = "ExerciseType";
            public const String WeightPropertyName = "Weight";
            public const String RepsPropertyName = "Reps";
            public const String SetsPropertyName = "Sets";

            #endregion End of Members
        }


        #endregion End of Private Classes
    }

    /// <summary>
    /// Enumeration for exercise type
    /// </summary>
    public enum ExerciseType
    {
        eDumbbellChestPress,
        eDumbbellPullover,
        eDumbbellShoulderPress,
        eDumbbellBicepCurl,
        eTripcepKickback,
        eWristCurl,
        eReverseWristCurl,
        eCrunch,
        eSquat,
        eLunge
    };

    #region Converters

    [ValueConversion(typeof(ExerciseType), typeof(String))]
    public class ExerciseTypeToStringConverter : IValueConverter
    {
        public Object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            String typeStr = "UNKNOWN TYPE";

            if (value is ExerciseType)
            {
                typeStr = Application.Current.FindResource(value.ToString() + ".Description") as String;
            }

            return typeStr;
        }

        public Object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    #endregion End of Converters
}
