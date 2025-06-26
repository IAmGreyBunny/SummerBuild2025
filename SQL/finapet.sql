-- phpMyAdmin SQL Dump
-- version 5.1.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost:8889
-- Generation Time: Jun 26, 2025 at 03:54 PM
-- Server version: 5.7.24
-- PHP Version: 8.3.1

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `finapet`
--

-- --------------------------------------------------------

--
-- Table structure for table `inventory`
--

CREATE TABLE `inventory` (
  `inventory_id` int(11) NOT NULL,
  `player_id` int(11) NOT NULL,
  `item_id` int(11) NOT NULL,
  `quantity` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `inventory`
--

INSERT INTO `inventory` (`inventory_id`, `player_id`, `item_id`, `quantity`) VALUES
(10, 10, 1, 7),
(17, 24, 1, 2),
(19, 23, 1, 17);

-- --------------------------------------------------------

--
-- Table structure for table `items`
--

CREATE TABLE `items` (
  `item_id` int(11) NOT NULL,
  `item_name` varchar(255) NOT NULL,
  `item_price` int(11) NOT NULL DEFAULT '0',
  `item_type` varchar(20) NOT NULL DEFAULT 'consumable'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `items`
--

INSERT INTO `items` (`item_id`, `item_name`, `item_price`, `item_type`) VALUES
(1, 'Feed', 1, 'consumable'),
(2, 'Border Collie', 20, 'pet'),
(3, 'Siamese Cat', 25, 'pet'),
(4, 'Chicken', 15, 'pet');

-- --------------------------------------------------------

--
-- Table structure for table `pets`
--

CREATE TABLE `pets` (
  `pet_id` int(11) NOT NULL,
  `owner_id` int(11) NOT NULL,
  `pet_type` int(11) NOT NULL,
  `hunger` int(11) DEFAULT '100',
  `affection` int(11) DEFAULT '100',
  `last_fed` datetime DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `pets`
--

INSERT INTO `pets` (`pet_id`, `owner_id`, `pet_type`, `hunger`, `affection`, `last_fed`) VALUES
(2, 10, 0, 84, 55, '2025-06-25 19:32:21'),
(3, 10, 0, 82, 100, '2025-06-26 04:01:57'),
(4, 10, 2, 82, 100, '2025-06-26 04:02:02'),
(5, 10, 1, 84, 100, '2025-06-26 06:43:06'),
(6, 10, 1, 84, 100, '2025-06-26 06:43:07'),
(7, 10, 1, 84, 100, '2025-06-26 06:43:07'),
(8, 10, 1, 84, 100, '2025-06-26 06:43:07'),
(9, 10, 1, 84, 100, '2025-06-26 06:43:07'),
(10, 10, 1, 84, 100, '2025-06-26 06:43:08'),
(11, 10, 1, 84, 100, '2025-06-26 06:43:08'),
(12, 10, 1, 84, 100, '2025-06-26 06:43:08'),
(13, 10, 1, 84, 100, '2025-06-26 06:43:08'),
(14, 10, 1, 84, 100, '2025-06-26 06:43:09'),
(15, 10, 1, 84, 100, '2025-06-26 06:43:09'),
(16, 23, 0, 89, 100, '2025-06-26 14:40:34'),
(17, 24, 2, 88, 100, '2025-06-26 10:24:25');

-- --------------------------------------------------------

--
-- Table structure for table `players`
--

CREATE TABLE `players` (
  `id` int(10) NOT NULL,
  `username` varchar(32) NOT NULL,
  `email` varchar(128) NOT NULL,
  `hash` varchar(100) NOT NULL,
  `salt` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `players`
--

INSERT INTO `players` (`id`, `username`, `email`, `hash`, `salt`) VALUES
(10, 'Test', 'Test@gmail.com', '$5$rounds=5000$steamedhamsTest$1Y7FX3mFTocI9qzIsncg3J/X9Rxn9NtWkFhTCfS9neD', '$5$rounds=5000$steamedhamsTest$'),
(15, 'plsWork', 'work@test.com', '$5$rounds=5000$steamedhamsplsWo$UlrAM9HDilNED4ZTfrxy0O8snNFBBsm1QE0vd9xuCL2', '$5$rounds=5000$steamedhamsplsWork$'),
(16, 'hihi', 'hellooo123@gmail.com', '$5$rounds=5000$steamedhamshihi$AEnGuFok2n1XgIiNzPt9AqgQ0c1NAHdwd9SICJRjh56', '$5$rounds=5000$steamedhamshihi$'),
(17, 'ILoveNUGS', 'nugs@gmail.com', '$5$rounds=5000$steamedhamsILove$N5P2HTkfWuH0jB/hPeQpLzs0FBXrnk.uj5VAf08/p.2', '$5$rounds=5000$steamedhamsILoveNUGS$'),
(18, 'helpme', 'me@gmail.com', '$5$rounds=5000$steamedhamshelpm$i9tQIMkJQQ8u3Jg01Gl3.ZYZz1c.WGVz1KQBZtvqm6/', '$5$rounds=5000$steamedhamshelpme$'),
(19, 'gg', 'gg@gmail.com', '$5$rounds=5000$steamedhamsgg$ux9sUE9RX.nsm8bDmWUlkt807MPd3NrQmVVCtgT25Q0', '$5$rounds=5000$steamedhamsgg$'),
(20, 'g', 'g@hotmail.com', '$5$rounds=5000$steamedhamsg$IpAronfjOpXa8JFY9RnK808ySD8r7OwoXlAjA7asG56', '$5$rounds=5000$steamedhamsg$'),
(21, 'newUser', 'nu@u.com', '$5$rounds=5000$steamedhamsnewUs$xGKlF23/WHajj0saZANECCh69hQiNGSokWDe.7mIUjA', '$5$rounds=5000$steamedhamsnewUser$'),
(22, 'anotha', 'a@u.com', '$5$rounds=5000$steamedhamsanoth$TrrF068T49yzTJh5nvZF8Inm2kvd/D4RI9ph67mtFc3', '$5$rounds=5000$steamedhamsanotha$'),
(23, 'thor', 't@h.com', '$5$rounds=5000$steamedhamsthor$Zf2aq8O0FMWeEcOTzI5j9Cp1Mi5onfFmX87g64MS5BA', '$5$rounds=5000$steamedhamsthor$'),
(24, 'devhub', 'devhub@demo.com', '$5$rounds=5000$steamedhamsdevhu$8eGxe8qicXYaNKF/gLTOispoG3uKai8IkGFc7PkxnX5', '$5$rounds=5000$steamedhamsdevhub$'),
(25, 'introVid', 'iv@mail.com', '$5$rounds=5000$steamedhamsintro$RsVnA.aFcEaoRsa4whc9QEumrgxARbA.8Hw7u6cqBd.', '$5$rounds=5000$steamedhamsintroVid$');

--
-- Triggers `players`
--
DELIMITER $$
CREATE TRIGGER `after_player_insert` AFTER INSERT ON `players` FOR EACH ROW BEGIN
    INSERT INTO player_data (player_id)
    VALUES (NEW.id);
    
    INSERT INTO player_tracker_setting (player_id, monthly_income)
    VALUES (NEW.id, 0.00);
END
$$
DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `player_daily_tracker`
--

CREATE TABLE `player_daily_tracker` (
  `daily_tracker_id` int(11) NOT NULL,
  `player_id` int(11) NOT NULL,
  `daily_spending` decimal(10,2) DEFAULT '0.00',
  `last_updated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `player_daily_tracker`
--

INSERT INTO `player_daily_tracker` (`daily_tracker_id`, `player_id`, `daily_spending`, `last_updated`) VALUES
(16, 20, '50.00', '2025-06-25 21:08:15'),
(17, 15, '2.00', '2025-06-24 21:44:14'),
(18, 10, '1.00', '2025-06-25 21:43:09'),
(19, 15, '1.00', '2025-06-25 21:46:13'),
(20, 23, '2.00', '2025-06-25 22:19:52'),
(21, 24, '2.00', '2025-06-26 02:24:12'),
(22, 23, '1.00', '2025-06-26 14:46:25'),
(23, 10, '1.00', '2025-06-26 14:56:45'),
(24, 23, '1.00', '2025-06-26 14:59:06');

-- --------------------------------------------------------

--
-- Table structure for table `player_data`
--

CREATE TABLE `player_data` (
  `player_id` int(11) NOT NULL,
  `coin` int(11) DEFAULT '0',
  `avatar_sprite_id` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `player_data`
--

INSERT INTO `player_data` (`player_id`, `coin`, `avatar_sprite_id`) VALUES
(10, 4703, 1),
(15, 15, 1),
(16, 0, 0),
(17, 0, 0),
(18, 0, 0),
(19, 0, 0),
(20, 0, 0),
(21, 20, 0),
(22, 20, 0),
(23, 27, 1),
(24, 3, 0),
(25, 0, 0);

-- --------------------------------------------------------

--
-- Table structure for table `player_tracker_setting`
--

CREATE TABLE `player_tracker_setting` (
  `tracker_id` int(11) NOT NULL,
  `player_id` int(11) NOT NULL,
  `monthly_income` decimal(10,2) NOT NULL DEFAULT '0.00'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `player_tracker_setting`
--

INSERT INTO `player_tracker_setting` (`tracker_id`, `player_id`, `monthly_income`) VALUES
(1, 10, '200.00'),
(2, 15, '200.00'),
(3, 23, '250.00'),
(4, 24, '250.00'),
(5, 25, '0.00');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `inventory`
--
ALTER TABLE `inventory`
  ADD PRIMARY KEY (`inventory_id`),
  ADD UNIQUE KEY `unique_owner_item` (`player_id`,`item_id`),
  ADD UNIQUE KEY `unique_item_owner` (`player_id`,`item_id`),
  ADD KEY `item_id` (`item_id`);

--
-- Indexes for table `items`
--
ALTER TABLE `items`
  ADD PRIMARY KEY (`item_id`);

--
-- Indexes for table `pets`
--
ALTER TABLE `pets`
  ADD PRIMARY KEY (`pet_id`),
  ADD KEY `owner_id` (`owner_id`);

--
-- Indexes for table `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `username` (`username`),
  ADD UNIQUE KEY `email` (`email`);

--
-- Indexes for table `player_daily_tracker`
--
ALTER TABLE `player_daily_tracker`
  ADD PRIMARY KEY (`daily_tracker_id`),
  ADD KEY `fk_player_id` (`player_id`);

--
-- Indexes for table `player_data`
--
ALTER TABLE `player_data`
  ADD PRIMARY KEY (`player_id`);

--
-- Indexes for table `player_tracker_setting`
--
ALTER TABLE `player_tracker_setting`
  ADD PRIMARY KEY (`tracker_id`),
  ADD UNIQUE KEY `player_id` (`player_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `inventory`
--
ALTER TABLE `inventory`
  MODIFY `inventory_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

--
-- AUTO_INCREMENT for table `items`
--
ALTER TABLE `items`
  MODIFY `item_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `pets`
--
ALTER TABLE `pets`
  MODIFY `pet_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=18;

--
-- AUTO_INCREMENT for table `players`
--
ALTER TABLE `players`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

--
-- AUTO_INCREMENT for table `player_daily_tracker`
--
ALTER TABLE `player_daily_tracker`
  MODIFY `daily_tracker_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25;

--
-- AUTO_INCREMENT for table `player_tracker_setting`
--
ALTER TABLE `player_tracker_setting`
  MODIFY `tracker_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `inventory`
--
ALTER TABLE `inventory`
  ADD CONSTRAINT `inventory_ibfk_1` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `inventory_ibfk_2` FOREIGN KEY (`item_id`) REFERENCES `items` (`item_id`) ON DELETE CASCADE;

--
-- Constraints for table `pets`
--
ALTER TABLE `pets`
  ADD CONSTRAINT `pets_ibfk_1` FOREIGN KEY (`owner_id`) REFERENCES `players` (`id`);

--
-- Constraints for table `player_daily_tracker`
--
ALTER TABLE `player_daily_tracker`
  ADD CONSTRAINT `fk_player_id` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`);

--
-- Constraints for table `player_data`
--
ALTER TABLE `player_data`
  ADD CONSTRAINT `player_data_ibfk_1` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE;

--
-- Constraints for table `player_tracker_setting`
--
ALTER TABLE `player_tracker_setting`
  ADD CONSTRAINT `player_tracker_setting_ibfk_1` FOREIGN KEY (`player_id`) REFERENCES `players` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
