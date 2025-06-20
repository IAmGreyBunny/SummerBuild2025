<?php

// Connect to DB
require 'db_connect.php';

// Get input
$username = $_POST["username"];
$password = $_POST["password"];

// Get user data
$userCheckQuery = "SELECT hash FROM players WHERE username = '$username'";
$userCheck = mysqli_query($con, $userCheckQuery);

if (mysqli_num_rows($userCheck) == 0) {
    echo "2"; // Username not found
    exit();
}

// Get the hash from database
$row = mysqli_fetch_assoc($userCheck);
$storedHash = $row["hash"];

// Hash the incoming password the same way
$salt = "\$5\$rounds=5000\$" . "steamedhams" . $username . "\$";
$hashedPassword = crypt($password, $salt);

// Compare
if ($hashedPassword != $storedHash) {
    echo "3"; // Wrong password
    exit();
}

echo "0"; // Login success

?>