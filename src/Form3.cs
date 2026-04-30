using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace FoodOrderingSystem
{
    public partial class Form3 : Form
    {
        private readonly string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=app;Integrated Security=True";

        public Form3()
        {
            InitializeComponent();
        }

        // ─────────────────────────────────────────────
        // DELETE – removes customer by email + password
        // ─────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string email    = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both Email and Password.", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Step 1: Verify customer exists with matching email and password
                    int customerId = -1;
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT CID FROM Customer WHERE Email = @Email AND Password = @Password", con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email",    email);
                        checkCmd.Parameters.AddWithValue("@Password", password);
                        object result = checkCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show("No customer found with the provided email and password.",
                                "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        customerId = Convert.ToInt32(result);
                    }

                    // Step 2: Confirm deletion
                    DialogResult confirm = MessageBox.Show(
                        "Are you sure you want to delete this customer and all related data?",
                        "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm != DialogResult.Yes) return;

                    // Step 3: Delete phone numbers first (foreign key constraint)
                    using (SqlCommand deletePhonesCmd = new SqlCommand(
                        "DELETE FROM CustomerPhone WHERE CID = @CID", con))
                    {
                        deletePhonesCmd.Parameters.AddWithValue("@CID", customerId);
                        deletePhonesCmd.ExecuteNonQuery();
                    }

                    // Step 4: Delete the customer record
                    using (SqlCommand deleteCustomerCmd = new SqlCommand(
                        "DELETE FROM Customer WHERE CID = @CID", con))
                    {
                        deleteCustomerCmd.Parameters.AddWithValue("@CID", customerId);
                        deleteCustomerCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Customer and all related data deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the UI
                    btnShow_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // INSERT – registers a new customer
        // ─────────────────────────────────────────────
        private void btninsert_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Step 1: Check if email already exists
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM CUSTOMER WHERE EMAIL = @Email", con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        int emailCount = (int)checkCmd.ExecuteScalar();
                        if (emailCount > 0)
                        {
                            MessageBox.Show("This email is already registered. Please use a different email.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Step 2: Collect and validate phone numbers
                    List<string> newPhoneNumbers = new List<string>();
                    if (!string.IsNullOrWhiteSpace(txtPhoneNo1.Text)) newPhoneNumbers.Add(txtPhoneNo1.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(txtPhoneNo2.Text)) newPhoneNumbers.Add(txtPhoneNo2.Text.Trim());

                    if (newPhoneNumbers.Count == 0)
                    {
                        MessageBox.Show("A customer must provide at least one phone number.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Step 3: Enforce max 2 phone numbers
                    if (newPhoneNumbers.Count > 2)
                    {
                        MessageBox.Show("A customer can have at most 2 phone numbers.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Step 4: Insert customer and capture generated CID
                    int customerId;
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO CUSTOMER (EMAIL, PASSWORD, FIRSTNAME, LASTNAME, GOVERNMENT, CITY, STREET, DISTRICT) " +
                        "OUTPUT INSERTED.CID " +
                        "VALUES (@Email, @Password, @FirstName, @LastName, @Government, @City, @Street, @District)", con))
                    {
                        cmd.Parameters.AddWithValue("@Email",      txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Password",   txtPassword.Text);
                        cmd.Parameters.AddWithValue("@FirstName",  txtFName.Text);
                        cmd.Parameters.AddWithValue("@LastName",   txtLName.Text);
                        cmd.Parameters.AddWithValue("@Government", txtGovern.Text);
                        cmd.Parameters.AddWithValue("@City",       txtCity.Text);
                        cmd.Parameters.AddWithValue("@Street",     txtStreet.Text);
                        cmd.Parameters.AddWithValue("@District",   txtDistrict.Text);

                        object result = cmd.ExecuteScalar();
                        customerId = Convert.ToInt32(result);

                        MessageBox.Show("Customer inserted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Step 5: Insert phone numbers
                    using (SqlCommand phoneCmd = new SqlCommand(
                        "INSERT INTO CustomerPhone (CID, PhoneNo) VALUES (@CID, @PhoneNo)", con))
                    {
                        phoneCmd.Parameters.AddWithValue("@CID", customerId);
                        phoneCmd.Parameters.Add("@PhoneNo", SqlDbType.VarChar);

                        foreach (string phoneNumber in newPhoneNumbers)
                        {
                            try
                            {
                                phoneCmd.Parameters["@PhoneNo"].Value = phoneNumber;
                                phoneCmd.ExecuteNonQuery();
                            }
                            catch (SqlException ex) when (ex.Number == 2627) // Primary key violation
                            {
                                MessageBox.Show(
                                    $"The phone number '{phoneNumber}' is already associated with this customer.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    MessageBox.Show("Phone numbers inserted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Step 6: Refresh grid
                    RefreshTable(txtEmail.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // UPDATE – modifies customer information
        // ─────────────────────────────────────────────
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                string email         = txtEmail.Text.Trim();
                string inputPassword = txtPassword.Text.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Please enter the Email.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Fetch current customer data
                    SqlCommand getCmd = new SqlCommand(
                        "SELECT c.*, " +
                        "MIN(p.PhoneNo) AS Phone1, " +
                        "MAX(p.PhoneNo) AS Phone2 " +
                        "FROM Customer c " +
                        "LEFT JOIN CustomerPhone p ON c.CID = p.CID " +
                        "WHERE c.Email = @Email " +
                        "GROUP BY c.CID, c.FirstName, c.LastName, c.Password, " +
                        "c.Government, c.City, c.Street, c.District, c.Email", con);
                    getCmd.Parameters.AddWithValue("@Email", email);

                    SqlDataAdapter adapter = new SqlDataAdapter(getCmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No customer found with this email.", "Not Found",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DataRow original       = dt.Rows[0];
                    int    customerId      = Convert.ToInt32(original["CID"]);
                    string originalPassword = original["Password"].ToString();

                    // ── CASE 1: Password-only update ──
                    bool isPasswordChangeOnly = !string.IsNullOrWhiteSpace(inputPassword)
                                               && inputPassword != originalPassword
                                               && AllOtherFieldsEmpty();

                    if (isPasswordChangeOnly)
                    {
                        SqlCommand updatePwdCmd = new SqlCommand(
                            "UPDATE Customer SET Password = @Password WHERE Email = @Email", con);
                        updatePwdCmd.Parameters.AddWithValue("@Password", inputPassword);
                        updatePwdCmd.Parameters.AddWithValue("@Email",    email);
                        updatePwdCmd.ExecuteNonQuery();

                        MessageBox.Show("Password updated successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // ── CASE 2: Update other fields (requires correct current password) ──
                    if (string.IsNullOrWhiteSpace(inputPassword) || inputPassword != originalPassword)
                    {
                        MessageBox.Show("To update personal info, please enter the correct current password.",
                            "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Fall back to original values for any empty field
                    string newPassword   = (txtPassword.Text == originalPassword || string.IsNullOrWhiteSpace(txtPassword.Text))
                                          ? originalPassword : txtPassword.Text;
                    string newFirstName  = string.IsNullOrWhiteSpace(txtFirstName.Text)  ? original["FirstName"].ToString()  : txtFirstName.Text;
                    string newLastName   = string.IsNullOrWhiteSpace(txtLastName.Text)   ? original["LastName"].ToString()   : txtLastName.Text;
                    string newGovernment = string.IsNullOrWhiteSpace(txtGovernment.Text) ? original["Government"].ToString() : txtGovernment.Text;
                    string newCity       = string.IsNullOrWhiteSpace(txtCity.Text)       ? original["City"].ToString()       : txtCity.Text;
                    string newStreet     = string.IsNullOrWhiteSpace(txtStreet.Text)     ? original["Street"].ToString()     : txtStreet.Text;
                    string newDistrict   = string.IsNullOrWhiteSpace(txtDistrict.Text)   ? original["District"].ToString()   : txtDistrict.Text;

                    if (MessageBox.Show("Confirm update?", "Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    SqlCommand updateCmd = new SqlCommand(
                        "UPDATE Customer SET Password=@Password, FirstName=@FirstName, LastName=@LastName, " +
                        "Government=@Government, City=@City, Street=@Street, District=@District " +
                        "WHERE CID=@CID", con);
                    updateCmd.Parameters.AddWithValue("@Password",   newPassword);
                    updateCmd.Parameters.AddWithValue("@FirstName",  newFirstName);
                    updateCmd.Parameters.AddWithValue("@LastName",   newLastName);
                    updateCmd.Parameters.AddWithValue("@Government", newGovernment);
                    updateCmd.Parameters.AddWithValue("@City",       newCity);
                    updateCmd.Parameters.AddWithValue("@Street",     newStreet);
                    updateCmd.Parameters.AddWithValue("@District",   newDistrict);
                    updateCmd.Parameters.AddWithValue("@CID",        customerId);
                    updateCmd.ExecuteNonQuery();

                    // Update phone numbers if provided
                    bool hasPhone1 = !string.IsNullOrWhiteSpace(txtPhoneNo1.Text);
                    bool hasPhone2 = !string.IsNullOrWhiteSpace(txtPhoneNo2.Text);

                    if (hasPhone1 || hasPhone2)
                    {
                        new SqlCommand("DELETE FROM CustomerPhone WHERE CID=@CID", con)
                        {
                            Parameters = { new SqlParameter("@CID", customerId) }
                        }.ExecuteNonQuery();

                        SqlCommand insertPhone = new SqlCommand(
                            "INSERT INTO CustomerPhone (CID, PhoneNo) VALUES (@CID, @PhoneNo)", con);
                        insertPhone.Parameters.Add("@CID",    SqlDbType.Int).Value = customerId;
                        insertPhone.Parameters.Add("@PhoneNo", SqlDbType.VarChar);

                        if (hasPhone1)
                        {
                            insertPhone.Parameters["@PhoneNo"].Value = txtPhoneNo1.Text.Trim();
                            insertPhone.ExecuteNonQuery();
                        }
                        if (hasPhone2)
                        {
                            insertPhone.Parameters["@PhoneNo"].Value = txtPhoneNo2.Text.Trim();
                            insertPhone.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Customer updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    btnShow_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // SHOW DATA – displays all customers
        // ─────────────────────────────────────────────
        private void btnShow_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Load Customer table
                    using (SqlDataAdapter customerAdapter = new SqlDataAdapter("SELECT * FROM Customer", con))
                    {
                        DataTable customerTable = new DataTable();
                        customerAdapter.Fill(customerTable);
                        dataGridView2.DataSource = customerTable;
                    }

                    // Load CustomerPhone table
                    using (SqlDataAdapter phoneAdapter = new SqlDataAdapter("SELECT * FROM CustomerPhone", con))
                    {
                        DataTable phoneTable = new DataTable();
                        phoneAdapter.Fill(phoneTable);
                        dgvCustomer2.DataSource = phoneTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // HELPER – refreshes grid after insert
        // ─────────────────────────────────────────────
        private void RefreshTable(string email)
        {
            try
            {
                if (DesignMode) return;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(
                        "SELECT C.*, P.PhoneNo " +
                        "FROM CUSTOMER C " +
                        "LEFT JOIN CustomerPhone P ON C.CID = P.CID " +
                        "WHERE C.EMAIL = @Email", con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@Email", email);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        CustomerDataGrid.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while refreshing the table: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────
        // HELPER – checks if all non-password fields are empty
        // ─────────────────────────────────────────────
        private bool AllOtherFieldsEmpty()
        {
            return string.IsNullOrWhiteSpace(txtFirstName.Text)
                && string.IsNullOrWhiteSpace(txtLastName.Text)
                && string.IsNullOrWhiteSpace(txtGovernment.Text)
                && string.IsNullOrWhiteSpace(txtCity.Text)
                && string.IsNullOrWhiteSpace(txtStreet.Text)
                && string.IsNullOrWhiteSpace(txtDistrict.Text)
                && string.IsNullOrWhiteSpace(txtPhoneNo1.Text)
                && string.IsNullOrWhiteSpace(txtPhoneNo2.Text);
        }
    }
}
