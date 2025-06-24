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

$player_id = $json['player_id']?? null;
if(!$player_id)
{
	if($json === null){
	echo json_encode([
		"status_code" => 10,
		"error_message" => "player_id not provided"
	]);
	exit();
}
}

//Query Shop
$getInventoryQuery = "SELECT * FROM inventory INNER JOIN items ON inventory.item_id=items.item_id WHERE player_id='" . $player_id . "';";
$getInventoryQueryResult = mysqli_query($con,$getInventoryQuery);

if (!$getInventoryQueryResult) {
	echo json_encode([
		"status_code" => 11,
		"error_message" => "Get Inventory Query Failed"
	]);
	exit();
}
else
{

	// Collect data
	$items = [];
	while ($row = mysqli_fetch_assoc($getInventoryQueryResult)) {
		$items[] = $row;
	}
	

	echo json_encode([
		"status_code" => $status_code,
		"error_message" => $error_message,
    	"items" => $items
	]);
}

?>