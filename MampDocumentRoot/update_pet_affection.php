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

$pet_id = $json['pet_id'] ?? '';
if (!$pet_id) {
	echo json_encode([
		"status_code" => 17,
		"error_message" => "pet_id value not provided"
	]);
	exit();
}

$affection = $json['affection'] ?? '';
if (!$affection) {
	echo json_encode([
		"status_code" => 22,
		"error_message" => "affection value not provided"
	]);
	exit();
}

// Query for player_data
$updatePetAffectionQuery = "UPDATE pets SET affection = '" . $affection .  "' WHERE pet_id = '" . $pet_id . "';";
$updatePetAffectionQueryResult = mysqli_query($con, $updatePetAffectionQuery);

if (!$updatePetAffectionQueryResult) {
	echo json_encode([
		"status_code" => 23,
		"error_message" => "Update Pet Affection Query Failed"
	]);
	exit();
}

echo json_encode([
	"status_code" => $status_code,
	"error_message" => $error_message
]);
?>