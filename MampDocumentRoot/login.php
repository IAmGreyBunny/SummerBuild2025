<?php
    header('Content-Type: application/json');

    // Error info
    $status_code = 0;
    $error_message = "";

    // Connect to DB
    require 'db_connect.php';

    // Get input
    $username = $_POST["username"];
    $password = $_POST["password"];

    // Get user data
    $userCheckQuery = "SELECT id,hash FROM players WHERE username = '$username'";
    $userCheck = mysqli_query($con, $userCheckQuery);

    if (mysqli_num_rows($userCheck) == 0) {
        $status_code = 2;
        $error_message = "Username not found";

        echo json_encode([
        "status_code" => $status_code,
        "error_message" => $error_message
        ]);
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
        $status_code = 3; 
        $error_message = "Wrong password";

        echo json_encode([
        "status_code" => $status_code,
        "error_message" => $error_message
        ]);
        exit();
    }

    $player_id = $row["id"];

    echo json_encode([
        "status_code" => $status_code, 
        "error_message" => $error_message,
        "player_data" =>[
            "player_id" => $player_id,
            "username" => $username
        ]
    ]);
    exit();
    

?>