<?php
    header('Content-Type: application/json');
    require_once 'db_config.php';

    // Error info
    $status_code = 0;
    $error_message = "";

    // Connects to MySQL database
    $con = mysqli_connect('localhost', $db_username, $db_password, 'finapet');

    // Check connection
    if (mysqli_connect_errno()) {
        $status_code = 1;
        $error_message = "Database connection failed";

        echo json_encode([
        "status_code" => $status_code,
        "error_message" => $error_message
        ]);

        exit();
    }

?>