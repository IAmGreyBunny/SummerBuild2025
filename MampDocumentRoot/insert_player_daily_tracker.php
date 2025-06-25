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

$daily_spending = $json['daily_spending'] ?? null;
if($daily_spending === null)
{
	echo json_encode([
		"status_code" => 26,
		"error_message" => "daily_spending value not provided"
	]);
	exit();
}

//Query player_daily_tracker
$insertPlayerDailyTrackerQuery = "INSERT INTO player_daily_tracker (player_id, daily_spending) VALUES ('" . $player_id . "', '" . $daily_spending . "');";
$insertPlayerDailyTrackerQueryResult = mysqli_query($con,$insertPlayerDailyTrackerQuery);

if (!$insertPlayerDailyTrackerQueryResult) {
	echo json_encode([
		"status_code" => 30,
		"error_message" => "Insert Player Daily Tracker Query Failed"
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