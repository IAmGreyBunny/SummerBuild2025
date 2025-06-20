<?php
	//Connects to sql database
	require 'db_connect.php';

	//Parse variables from caller's POST request
	$username = $_POST['username'];
	$email = $_POST['email'];
	$password = $_POST['password'];
	
	//Check if username exists
	$usernameCheckQuery = "SELECT username FROM players WHERE username='" . $username . "';";
	$usernameQueryResult = mysqli_query($con,$usernameCheckQuery) or die("2: Username check query failed");

	if(mysqli_num_rows($usernameQueryResult)>0)
	{
		echo "3: Userame already exists";
		exit();
	}

	//Check if email exists
	$emailCheckQuery = "SELECT email FROM players WHERE email='" . $email . "';";
	$emailQueryResult = mysqli_query($con,$emailCheckQuery) or die("4: Email check query failed");

	if(mysqli_num_rows($emailQueryResult)>0)
	{
		echo "5: Email already exists";
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
	mysqli_query($con,$insertUserQuery) or die("6: Insert player query failed");

	echo "0";
?>