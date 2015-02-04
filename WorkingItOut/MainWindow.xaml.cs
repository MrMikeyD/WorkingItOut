using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;

namespace WorkingItOut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Events

        #endregion End of Events

        #region Methods

        /// <summary>
        /// CanExecute Handler for command
        /// </summary>]
        public void CanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            if ((e.Command == ApplicationCommands.New) ||
                (e.Command == ApplicationCommands.Open) ||
                (e.Command == ApplicationCommands.Save) ||
                (e.Command == ApplicationCommands.SaveAs) ||
                (e.Command == ApplicationCommands.Close))
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Execute Handler for command
        /// </summary>]
        public void Execute(Object sender, ExecutedRoutedEventArgs e)
        {
             if (e.Command == ApplicationCommands.New)
             {

             }
             else if (e.Command == ApplicationCommands.Open)
             {
                System.Windows.Forms.OpenFileDialog openFileDialog = new
                    System.Windows.Forms.OpenFileDialog();

                openFileDialog .InitialDirectory = "c:\\" ;
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*" ;
                openFileDialog.FilterIndex = 2 ;
                openFileDialog.RestoreDirectory = true ;

                if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        this.WorkoutList.LoadWorkoutFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
             }
             else if (e.Command == ApplicationCommands.Save)
             {

             }
             else if (e.Command == ApplicationCommands.SaveAs)
             {

             }
             else if (e.Command == ApplicationCommands.Close)
             {
                 
             }
        }

        #endregion End of Methods

        #region Properties

        #endregion End of Properties

        #region Members

        public static readonly RoutedCommand AddExerciseCommand = new RoutedCommand("AddExercise", typeof(MainWindow));
        public static readonly RoutedCommand RemoveExerciseCommand = new RoutedCommand("RemoveExercise", typeof(MainWindow));

        #endregion End of Members

    }
}
