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

//Query PlayerTrackerSetting
$getPlayerTrackerSettingQuery = "SELECT * FROM player_tracker_setting WHERE player_id='" . $player_id . "';";
$getPlayerTrackerSettingQueryResult = mysqli_query($con,$getPlayerTrackerSettingQuery);

if (!$getPlayerTrackerSettingQueryResult) {
	echo json_encode([
		"status_code" => 27,
		"error_message" => "Get Player Tracker Setting Query Failed"
	]);
	exit();
}
else
{

	// Collect data
	$player_tracker_setting = [];
	while ($row = mysqli_fetch_assoc($getPlayerTrackerSettingQueryResult)) {
		$player_tracker_setting[] = $row;
	}
	

	echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message,
    	"player_tracker_setting" => $player_tracker_setting
	]);
}

?>