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

$hunger = $json['hunger'] ?? '';
if (!$hunger) {
	echo json_encode([
		"status_code" => 19,
		"error_message" => "hunger value not provided"
	]);
	exit();
}

$last_fed = $json['last_fed'] ?? '';
if (!$last_fed) {
	echo json_encode([
		"status_code" => 20,
		"error_message" => "last_fed value not provided"
	]);
	exit();
}

// Query for player_data
$updatePetHungerQuery = "UPDATE pets SET hunger = '" . $hunger . "', last_fed = '" . $last_fed . "' WHERE pet_id = '" . $pet_id . "';";
$updatePetHungerQueryResult = mysqli_query($con, $updatePetHungerQuery);

if (!$updatePetHungerQueryResult) {
	echo json_encode([
		"status_code" => 21,
		"error_message" => "Update Pet Hunger Query Failed"
	]);
	exit();
}

echo json_encode([
	"status_code" => $status_code,
	"error_message" => $error_message
]);
?>