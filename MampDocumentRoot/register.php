<?php
	header('Content-Type: application/json');

    // Error info
    $status_code = 0;
    $error_message = "";

	//Connects to sql database
	require 'db_connect.php';

	//Parse variables from caller's POST request
	$username = $_POST['username'];
	$email = $_POST['email'];
	$password = $_POST['password'];
	
	//Check if username exists
	$usernameCheckQuery = "SELECT username FROM players WHERE username='" . $username . "';";
	$usernameQueryResult = mysqli_query($con,$usernameCheckQuery);

	if (!$usernameQueryResult) {
	    echo json_encode([
	        "status_code" => 2,
	        "error_message" => "Username check query failed"
	    ]);
	    exit();
	}

	if(mysqli_num_rows($usernameQueryResult)>0)
	{
		$status_code = 3; 
        $error_message = "Username already exists";

        echo json_encode([
        "status_code" => $status_code,
        "error_message" => $error_message
        ]);
		exit();
	}

	//Check if email exists
	$emailCheckQuery = "SELECT email FROM players WHERE email='" . $email . "';";
	$emailQueryResult = mysqli_query($con,$emailCheckQuery);


	if (!$emailQueryResult) {
	    echo json_encode([
	        "status_code" => 4,
	        "error_message" => "Email check query failed"
	    ]);
	    exit();
	}

	if(mysqli_num_rows($emailQueryResult)>0)
	{
		$status_code = 5; 
        $error_message = "Email already exists";

        echo json_encode([
        "status_code" => $status_code,
        "error_message" => $error_message
        ]);
		exit();
	}

	//Generating password hash to keep password stored secured
	$salt = "\$5\$rounds=5000\$" . "steamedhams" . $username . "\$"; //Generate unique salt based on username and sha-256
	$hash = crypt($password,$salt); //encrypt hash based on salt generated

	//Insert user into table
	$insertUserQuery = "INSERT INTO players (username,email,hash,salt) VALUES (
		'" . $username . "',
		'" . $email . "',
		'" . $hash . "',
		'" . $salt . "');";
	$insertUserQueryResult = mysqli_query($con,$insertUserQuery);
	if (!$insertUserQueryResult) {
    echo json_encode([
        "status_code" => 6,
        "error_message" => "Insert player query failed"
    ]);
    exit();
}

	echo json_encode([
        "status_code" => $status_code,
        "error_message" => $error_message
    ]);
?>