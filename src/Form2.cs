using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FoodOrderingSystem
{
    public partial class Form2 : Form
    {
        private readonly string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=app;Integrated Security=True";

        public Form2()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────────
        // DELETE – opens Form5 where user enters FeedbackID
        // ─────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            Form5 form5 = new Form5();
            form5.Show();
        }

        // ─────────────────────────────────────────────
        // INSERT – adds a new feedback record
        // ─────────────────────────────────────────────
        private void btninsert_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtMealId.Text) ||
                    string.IsNullOrWhiteSpace(txtRating.Text))
                {
                    MessageBox.Show("All fields are required.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(txtMealId.Text, out int mealId) || mealId <= 0)
                {
                    MessageBox.Show("Please enter a valid Meal ID.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(txtRating.Text, out int rating) || rating < 1 || rating > 5)
                {
                    MessageBox.Show("Please enter a valid rating (1-5).", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Check if MealID exists
                    using (SqlCommand checkMealCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Meal WHERE MealID = @MealID", con))
                    {
                        checkMealCmd.Parameters.Add("@MealID", SqlDbType.Int).Value = mealId;
                        int mealCount = (int)checkMealCmd.ExecuteScalar();
                        if (mealCount == 0)
                        {
                            MessageBox.Show("The Meal ID does not exist. Please enter a valid Meal ID.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Get CustomerID from email
                    int customerId;
                    using (SqlCommand getCustomerCmd = new SqlCommand(
                        "SELECT CID FROM CUSTOMER WHERE EMAIL = @Email", con))
                    {
                        getCustomerCmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = txtEmail.Text;
                        object result = getCustomerCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("This email is invalid. Please use a different email.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        customerId = Convert.ToInt32(result);
                    }

                    // Insert feedback
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO FEEDBACK (CID, MealID, Rating, Comment) VALUES (@CID, @MealID, @Rating, @Comment)", con))
                    {
                        cmd.Parameters.Add("@CID", SqlDbType.Int).Value = customerId;
                        cmd.Parameters.Add("@MealID", SqlDbType.Int).Value = mealId;
                        cmd.Parameters.Add("@Rating", SqlDbType.Int).Value = rating;
                        cmd.Parameters.Add("@Comment", SqlDbType.NVarChar).Value =
                            string.IsNullOrWhiteSpace(txtComment.Text) ? (object)DBNull.Value : txtComment.Text;
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Feedback inserted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    RefreshTable(customerId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // UPDATE – modifies an existing feedback record
        // ─────────────────────────────────────────────
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtMealId.Text) ||
                    string.IsNullOrWhiteSpace(txtRating.Text))
                {
                    MessageBox.Show("All fields are required.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(txtMealId.Text, out int mealId) || mealId <= 0)
                {
                    MessageBox.Show("Please enter a valid Meal ID.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(txtRating.Text, out int rating) || rating < 1 || rating > 5)
                {
                    MessageBox.Show("Please enter a valid rating (1-5).", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Check if MealID exists
                    using (SqlCommand checkMealCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Meal WHERE MealID = @MealID", con))
                    {
                        checkMealCmd.Parameters.Add("@MealID", SqlDbType.Int).Value = mealId;
                        int mealCount = (int)checkMealCmd.ExecuteScalar();
                        if (mealCount == 0)
                        {
                            MessageBox.Show("The Meal ID does not exist. Please enter a valid Meal ID.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Get CustomerID from email
                    int customerId;
                    using (SqlCommand getCustomerCmd = new SqlCommand(
                        "SELECT CID FROM CUSTOMER WHERE EMAIL = @Email", con))
                    {
                        getCustomerCmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = txtEmail.Text;
                        object result = getCustomerCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Invalid email. Please use a registered email.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        customerId = Convert.ToInt32(result);
                    }

                    // Perform the update
                    using (SqlCommand updateCmd = new SqlCommand(
                        "UPDATE FEEDBACK SET Rating = @Rating, Comment = @Comment WHERE CID = @CID AND MealID = @MealID", con))
                    {
                        updateCmd.Parameters.Add("@Rating", SqlDbType.Int).Value = rating;
                        updateCmd.Parameters.Add("@Comment", SqlDbType.NVarChar).Value =
                            string.IsNullOrWhiteSpace(txtComment.Text) ? (object)DBNull.Value : txtComment.Text;
                        updateCmd.Parameters.Add("@CID", SqlDbType.Int).Value = customerId;
                        updateCmd.Parameters.Add("@MealID", SqlDbType.Int).Value = mealId;

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Feedback updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshTable(customerId);
                        }
                        else
                        {
                            MessageBox.Show("No matching feedback found to update.", "Info",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating feedback: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // SHOW DATA – displays feedback for the entered email
        // ─────────────────────────────────────────────
        private void btnShow_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Please enter an email.", "Input Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Resolve email to CID
                    int customerId;
                    using (SqlCommand getCustomerCmd = new SqlCommand(
                        "SELECT CID FROM CUSTOMER WHERE EMAIL = @Email", con))
                    {
                        getCustomerCmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = txtEmail.Text;
                        object result = getCustomerCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("Email not found in the database.", "Not Found",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        customerId = Convert.ToInt32(result);
                    }

                    // Fetch feedbacks for this customer
                    using (SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT FeedbackID, MealID, Rating, Comment FROM FEEDBACK WHERE CID = @CID", con))
                    {
                        da.SelectCommand.Parameters.Add("@CID", SqlDbType.Int).Value = customerId;
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvFeedback.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading feedback data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // HELPER – refreshes the DataGridView after CUD
        // ─────────────────────────────────────────────
        private void RefreshTable(int customerId)
        {
            try
            {
                if (DesignMode) return;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT F.MealID, F.Rating, F.Comment FROM FEEDBACK F WHERE F.CID = @CID", con))
                    {
                        da.SelectCommand.Parameters.Add("@CID", SqlDbType.Int).Value = customerId;
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        FeedbackTable.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while refreshing the table: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
