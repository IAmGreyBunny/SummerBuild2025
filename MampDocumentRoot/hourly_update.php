<?php
header('Content-Type: application/json');

// Error info
$status_code = 0;
$error_message = "";

//Connects to sql database
require 'db_connect.php';

//Construct query depending on whether owner_id is specified, if
$updatePetsQuery = "UPDATE pets SET hunger = hunger - 1 WHERE last_fed < NOW() - INTERVAL 1 HOUR;";
$updatePetsQueryResult = mysqli_query($con,$updatePetsQuery);

if (!$updatePetsQueryResult) {
	$status_code = 13; 
    $error_message = "Update Pet Query Failed";

    echo json_encode([
    "status_code" => $status_code,
    "error_message" => $error_message
    ]);
	exit();
}
else
{
	echo json_encode([
    "status_code" => $status_code,
    "error_message" => $error_message
    ]);
	exit();
}

?>