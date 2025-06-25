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

$player_id = $json['player_id'] ?? '';
if (!$player_id) {
	echo json_encode([
		"status_code" => 10,
		"error_message" => "player_id not provided"
	]);
	exit();
}
$coin = $json['coin'] ?? '';
if ($coin === null) {
	echo json_encode([
		"status_code" => 14,
		"error_message" => "coin value not provided"
	]);
	exit();
}
$avatar_sprite_id = $json['avatar_sprite_id'] ?? '';
if ($avatar_sprite_id === null) {
	echo json_encode([
		"status_code" => 15,
		"error_message" => "avatar_sprite_id value not provided"
	]);
	exit();
}

// Query for player_data
$updatePlayerDataQuery = "UPDATE player_data SET coin = '" . $coin . "', avatar_sprite_id = '" . $avatar_sprite_id . "' WHERE player_id = '" . $player_id . "';";
$updatePlayerDataQueryResult = mysqli_query($con, $updatePlayerDataQuery);

if (!$updatePlayerDataQueryResult) {
	echo json_encode([
		"status_code" => 16,
		"error_message" => "Update Player Data Query Failed"
	]);
	exit();
}

echo json_encode([
	"status_code" => $status_code,
	"error_message" => $error_message
]);
?>