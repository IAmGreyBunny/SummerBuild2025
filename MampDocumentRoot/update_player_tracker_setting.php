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

$player_id = $json['player_id'] ?? null;
if(!$player_id)
{
	echo json_encode([
		"status_code" => 10,
		"error_message" => "player_id not provided"
	]);
	exit();

}

$monthly_income = $json['monthly_income'] ?? null;
if($monthly_income === null)
{
	echo json_encode([
		"status_code" => 26,
		"error_message" => "monthly_income value not provided"
	]);
	exit();
}

//Query PlayerTrackerSetting
$updatePlayerTrackerSettingQuery = "UPDATE player_tracker_setting SET monthly_income = '" . $monthly_income . "' WHERE player_id = '" . $player_id . "';";
$updatePlayerTrackerSettingQueryResult = mysqli_query($con,$updatePlayerTrackerSettingQuery);

if (!$updatePlayerTrackerSettingQueryResult) {
	echo json_encode([
		"status_code" => 28,
		"error_message" => "Update Player Tracker Setting Query Failed"
	]);
	exit();
}
else
{
	echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message
	]);
}

?>