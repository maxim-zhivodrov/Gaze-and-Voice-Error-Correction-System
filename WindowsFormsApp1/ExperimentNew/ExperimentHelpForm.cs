using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeGaze
{
    public partial class ExperimentHelpForm : Form
    {
        public ExperimentHelpForm()
        {
            InitializeComponent();
        }
        private void fix_Click(object sender, EventArgs e)
        {
            // Fix
            this.exampleLabel.Text = "Command: \"Fix\"";
            this.explainLabel.Text = "Fix Non-Words in document to the first good option.";
        }

        private void replace_Click(object sender, EventArgs e)
        {
            // Replace
            this.exampleLabel.Text = "Command: \"Replace <WordToReplace> <WordToReplaceTo>\"";
            this.explainLabel.Text = "Replaces a particular word that exists in the document with the new word.";
        }

        private void add_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Add <WordsToAdd> After <WordsToAdd>\"";
            this.explainLabel.Text = "Add a word or sequence of words after a specific word that exists in the document";
        }

        private void delete_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Delete From <FirstWord> -> To <LastWord>\"";
            this.explainLabel.Text = "Delete a specific word sequence exists in the document";
        }

        private void delete_from_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Delete From <FirstWord> -> To <LastWord>\"";
            this.explainLabel.Text = "Delete a phrase that starts from one word to another";
        }

        private void copy_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Copy <WordToCopy>\"";
            this.explainLabel.Text = "Copy specific word/word sequence exists in the document";
        }

        private void copy_from_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Copy From <WordToCopyFrom> To <WordToCopyTo>\"";
            this.explainLabel.Text = "Copy phrase that starts from one word to another";
        }

        private void past_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Paste After\\Before <WordToPast After\\Before>\"";
            this.explainLabel.Text = "Pastes the phrase after a Copy command";
        }

        private void more_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Options\"";
            this.explainLabel.Text = "After Fix Command. Displays 5 more correction options for the correction.";
        }

        private void no_Click(object sender, EventArgs e)
        {
            this.exampleLabel.Text = "Command: \"Cancel\"";
            this.explainLabel.Text = "Undo the last command.";

        }

        private void CloseHelpBtn_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
