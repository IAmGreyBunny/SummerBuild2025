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

// Query for player_data
$getPlayerDataQuery = "SELECT * FROM player_data WHERE player_id='" . $player_id . "';";
$getPlayerDataQueryResult = mysqli_query($con, $getPlayerDataQuery);

if (!$getPlayerDataQueryResult) {
	echo json_encode([
		"status_code" => 12,
		"error_message" => "Get Player Data Query Failed"
	]);
	exit();
}

// Collect data
$player_data = [];
while ($row = mysqli_fetch_assoc($getPlayerDataQueryResult)) {
	$player_data[] = $row;
}

echo json_encode([
	"status_code" => $status_code,
	"error_message" => $error_message,
	"player_data" => $player_data
]);
?>