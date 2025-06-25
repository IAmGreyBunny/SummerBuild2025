<?php
header('Content-Type: application/json');

// Error info
$status_code = 0;
$error_message = "";

//Connects to sql database
require 'db_connect.php';

//Parse variables from caller's POST request
$json = json_decode(file_get_contents('php://input'),true);
if($json === null){
	echo json_encode([
		"status_code" => 7,
		"error_message" => "Invalid JSON"
	]);
	exit();
}

$pet_id = $json['pet_id'] ?? '';
//Construct query
if($pet_id)
{
	$dropPetQuery = "DROP * FROM pets WHERE pet_id='" . $pet_id . "';";
}
else
{
	echo json_encode([
		"status_code" => 17,
		"error_message" => "pet_id value not provided"
	]);
	exit();
}
$dropPetQueryResult = mysqli_query($con,$dropPetQuery);

if (!$dropPetQuery) {
	echo json_encode([
		"status_code" => 18,
		"error_message" => "Drop Pet Query Failed"
	]);
	exit();
}

echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message
	]);

?>