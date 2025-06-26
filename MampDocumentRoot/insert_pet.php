<?php
header('Content-Type: application/json');

// Error info
$status_code = 0;
$error_message = "";

// Connect to SQL database
require 'db_connect.php';

// Parse JSON input
$json = json_decode(file_get_contents('php://input'), true);
if ($json === null) {
	echo json_encode([
		"status_code" => 7,
		"error_message" => "Invalid JSON"
	]);
	exit();
}

$owner_id = $json['owner_id'] ?? '';
if ($owner_id === null) {
	echo json_encode([
		"status_code" => 10,
		"error_message" => "player_id not provided"
	]);
	exit();
}

$pet_type = $json['pet_type'] ?? '';
if ($pet_type === null) {
	echo json_encode([
		"status_code" => 24,
		"error_message" => "pet_type value not provided"
	]);
	exit();
}

// Query
$insertPetQuery = "INSERT INTO pets (owner_id, pet_type) VALUES ('" . $owner_id . "', '" . $pet_type . "');";
$insertPetQueryResult = mysqli_query($con, $insertPetQuery);

if (!$insertPetQueryResult) {
	echo json_encode([
		"status_code" => 25,
		"error_message" => "Insert Pet Query Failed"
	]);
	exit();
}

echo json_encode([
	"status_code" => $status_code,
	"error_message" => $error_message
]);
?>