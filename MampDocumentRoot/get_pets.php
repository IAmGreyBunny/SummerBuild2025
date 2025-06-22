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

$owner_id = $json['owner_id'] ?? '';
//Construct query depending on whether owner_id is specified, if
if($owner_id)
{
	$getPetsQuery = "SELECT * FROM pets WHERE owner_id='" . $owner_id . "';";
}
else
{
	$getPetsQuery = "SELECT * FROM pets";
}
$getPetsQueryResult = mysqli_query($con,$getPetsQuery);

if (!$getPetsQueryResult) {
	echo json_encode([
		"status_code" => 8,
		"error_message" => "Get Pets Query Failed"
	]);
	exit();
}
else
{

	// Collect data
	$pets = [];
	while ($row = mysqli_fetch_assoc($getPetsQueryResult)) {
		$pets[] = $row;
	}
	

	echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message,
    	"pets" => $pets
	]);
}

?>