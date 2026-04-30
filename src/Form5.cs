using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FoodOrderingSystem
{
    public partial class Form5 : Form
    {
        private readonly string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=app;Integrated Security=True";

        public Form5()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────────
        // DELETE – removes a feedback entry by FeedbackID
        // ─────────────────────────────────────────────
        private void btnDeleteFeedback_Click(object sender, EventArgs e)
        {
            // 1. Validate that something was entered
            if (string.IsNullOrWhiteSpace(txtFeedbackID.Text))
            {
                MessageBox.Show("Please enter a Feedback ID.", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validate that it is a positive integer
            if (!int.TryParse(txtFeedbackID.Text.Trim(), out int feedbackId) || feedbackId <= 0)
            {
                MessageBox.Show("Feedback ID must be a positive number.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 3. Check if the Feedback ID exists
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Feedback WHERE FeedbackID = @FeedbackID", con))
                    {
                        checkCmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists == 0)
                        {
                            MessageBox.Show("No feedback found with the given Feedback ID.", "Not Found",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    // 4. Confirm deletion
                    DialogResult confirm = MessageBox.Show(
                        "Are you sure you want to permanently delete this feedback?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (confirm != DialogResult.Yes) return;

                    // 5. Perform the deletion
                    using (SqlCommand deleteCmd = new SqlCommand(
                        "DELETE FROM Feedback WHERE FeedbackID = @FeedbackID", con))
                    {
                        deleteCmd.Parameters.AddWithValue("@FeedbackID", feedbackId);
                        int rowsAffected = deleteCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Feedback deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtFeedbackID.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
