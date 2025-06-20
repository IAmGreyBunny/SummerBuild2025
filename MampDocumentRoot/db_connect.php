<?php

// Connects to MySQL database
$con = mysqli_connect('localhost', 'root', 'root', 'finapet');

// Check connection
if (mysqli_connect_errno()) {
    echo "1: Database connection failed"; // Error code 1 = connection failed
    exit();
}

?>