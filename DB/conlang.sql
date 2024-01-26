-- MySQL dump 10.13  Distrib 8.0.33, for Win64 (x86_64)
--
-- Host: localhost    Database: conlang
-- ------------------------------------------------------
-- Server version	8.0.33

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `axes_rule_override`
--

DROP TABLE IF EXISTS `axes_rule_override`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `axes_rule_override` (
  `id` int NOT NULL AUTO_INCREMENT,
  `rule_override` int NOT NULL,
  `axis_value` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `rule_override_idx` (`rule_override`),
  KEY `axis_value_idx` (`axis_value`),
  CONSTRAINT `aro_axis_value` FOREIGN KEY (`axis_value`) REFERENCES `axis_value` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `aro_rule_override` FOREIGN KEY (`rule_override`) REFERENCES `rule_override` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=333 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `axis_name`
--

DROP TABLE IF EXISTS `axis_name`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `axis_name` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `an_language_idx` (`language`),
  CONSTRAINT `an_language` FOREIGN KEY (`language`) REFERENCES `language` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `axis_value`
--

DROP TABLE IF EXISTS `axis_value`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `axis_value` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `axis` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `axis_idx` (`axis`),
  CONSTRAINT `av_axis` FOREIGN KEY (`axis`) REFERENCES `axis_name` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `class_name`
--

DROP TABLE IF EXISTS `class_name`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `class_name` (
  `name` varchar(255) NOT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`name`,`language`),
  KEY `cn_language_idx` (`language`),
  CONSTRAINT `cn_language` FOREIGN KEY (`language`) REFERENCES `language` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `class_value`
--

DROP TABLE IF EXISTS `class_value`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `class_value` (
  `name` varchar(255) NOT NULL,
  `class` varchar(255) NOT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`name`,`class`,`language`),
  KEY `cv_class_idx` (`class`,`language`),
  CONSTRAINT `cv_class` FOREIGN KEY (`class`, `language`) REFERENCES `class_name` (`name`, `language`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `classes_rule`
--

DROP TABLE IF EXISTS `classes_rule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `classes_rule` (
  `id` int NOT NULL AUTO_INCREMENT,
  `rule` int NOT NULL,
  `class` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `a_r_rule_idx` (`rule`),
  KEY `a_r_class_idx` (`class`),
  CONSTRAINT `c_r_class` FOREIGN KEY (`class`) REFERENCES `class_value` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `c_r_rule` FOREIGN KEY (`rule`) REFERENCES `rule` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `classes_rule_override`
--

DROP TABLE IF EXISTS `classes_rule_override`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `classes_rule_override` (
  `id` int NOT NULL AUTO_INCREMENT,
  `rule_override` int NOT NULL,
  `class` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `cl_r_o_class_idx` (`class`),
  KEY `cl_r_o_rule_override_idx` (`rule_override`),
  CONSTRAINT `cl_r_o_class` FOREIGN KEY (`class`) REFERENCES `class_value` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `cl_r_o_rule_override` FOREIGN KEY (`rule_override`) REFERENCES `rule_override` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=64 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inflection`
--

DROP TABLE IF EXISTS `inflection`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inflection` (
  `id` int NOT NULL AUTO_INCREMENT,
  `speech_part` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `speech_part_idx` (`speech_part`),
  CONSTRAINT `i_speech_part` FOREIGN KEY (`speech_part`) REFERENCES `speech_part` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inflection_axes`
--

DROP TABLE IF EXISTS `inflection_axes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inflection_axes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `axis` int NOT NULL,
  `inflection` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `fk_inflection_axes_inflection_idx` (`inflection`),
  KEY `fk_inflection_axes_axis_name_idx` (`axis`),
  CONSTRAINT `fk_inflection_axes_axis_name` FOREIGN KEY (`axis`) REFERENCES `axis_name` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_inflection_axes_inflection` FOREIGN KEY (`inflection`) REFERENCES `inflection` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inflection_class`
--

DROP TABLE IF EXISTS `inflection_class`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inflection_class` (
  `id` int NOT NULL AUTO_INCREMENT,
  `inflection` int NOT NULL,
  `class` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `ic_class_idx` (`class`),
  KEY `ic_inflection_idx` (`inflection`),
  CONSTRAINT `ic_class` FOREIGN KEY (`class`) REFERENCES `class_value` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `ic_inflection` FOREIGN KEY (`inflection`) REFERENCES `inflection` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `language`
--

DROP TABLE IF EXISTS `language`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `language` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `phoneme`
--

DROP TABLE IF EXISTS `phoneme`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `phoneme` (
  `phoneme` varchar(255) NOT NULL,
  PRIMARY KEY (`phoneme`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `phoneme_class`
--

DROP TABLE IF EXISTS `phoneme_class`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `phoneme_class` (
  `id` int NOT NULL AUTO_INCREMENT,
  `key` char(1) NOT NULL,
  `parent` char(1) NOT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `phoneme_class_language_idx` (`language`) /*!80000 INVISIBLE */,
  CONSTRAINT `phoneme_class_language` FOREIGN KEY (`language`) REFERENCES `language` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `phoneme_class_phoneme`
--

DROP TABLE IF EXISTS `phoneme_class_phoneme`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `phoneme_class_phoneme` (
  `id` int NOT NULL AUTO_INCREMENT,
  `phoneme` varchar(255) NOT NULL,
  `class` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `phoneme_class_phoneme_phoneme_idx` (`phoneme`),
  CONSTRAINT `phoneme_class_phoneme_phoneme` FOREIGN KEY (`phoneme`) REFERENCES `phoneme` (`phoneme`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rule`
--

DROP TABLE IF EXISTS `rule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rule` (
  `id` int NOT NULL AUTO_INCREMENT,
  `rule` varchar(255) NOT NULL,
  `axis` int NOT NULL,
  `speech_part` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `r_axis_idx` (`axis`),
  KEY `r_speech_part_idx` (`speech_part`),
  KEY `rule_speech_part_idx` (`speech_part`),
  CONSTRAINT `r_axis` FOREIGN KEY (`axis`) REFERENCES `axis_value` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `r_speech_part` FOREIGN KEY (`speech_part`) REFERENCES `speech_part` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rule_override`
--

DROP TABLE IF EXISTS `rule_override`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rule_override` (
  `id` int NOT NULL AUTO_INCREMENT,
  `rule` varchar(255) NOT NULL,
  `speech_part` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `r_o_speech_part_idx` (`speech_part`),
  KEY `rule_o_speech_part_idx` (`speech_part`),
  CONSTRAINT `r_o_speech_part` FOREIGN KEY (`speech_part`) REFERENCES `speech_part` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `speech_part`
--

DROP TABLE IF EXISTS `speech_part`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `speech_part` (
  `name` varchar(255) NOT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`name`,`language`),
  UNIQUE KEY `name_UNIQUE` (`name`),
  KEY `sp_language_idx` (`language`),
  CONSTRAINT `sp_language` FOREIGN KEY (`language`) REFERENCES `language` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `syllable`
--

DROP TABLE IF EXISTS `syllable`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `syllable` (
  `id` int NOT NULL,
  `syllable` varchar(255) DEFAULT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_syllable_language1_idx` (`language`),
  CONSTRAINT `fk_syllable_language1` FOREIGN KEY (`language`) REFERENCES `language` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `term`
--

DROP TABLE IF EXISTS `term`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `term` (
  `id` int NOT NULL AUTO_INCREMENT,
  `word` varchar(255) NOT NULL,
  `transcription` varchar(255) DEFAULT NULL,
  `speech_part` varchar(255) DEFAULT NULL,
  `inflection` text,
  `language` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `speech_part_idx` (`speech_part`),
  KEY `t_language_idx` (`language`),
  CONSTRAINT `t_language` FOREIGN KEY (`language`) REFERENCES `language` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `t_speech_part` FOREIGN KEY (`speech_part`) REFERENCES `speech_part` (`name`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `term_class`
--

DROP TABLE IF EXISTS `term_class`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `term_class` (
  `id` int NOT NULL AUTO_INCREMENT,
  `term` int NOT NULL,
  `class` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `tc_term_idx` (`term`),
  KEY `tc_class_idx` (`class`),
  CONSTRAINT `tc_class` FOREIGN KEY (`class`) REFERENCES `class_value` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `tc_term` FOREIGN KEY (`term`) REFERENCES `term` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `transcription_rule`
--

DROP TABLE IF EXISTS `transcription_rule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `transcription_rule` (
  `id` int NOT NULL AUTO_INCREMENT,
  `rule` varchar(255) NOT NULL,
  `language` int NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`),
  KEY `tr_language_idx` (`language`),
  CONSTRAINT `tr_language` FOREIGN KEY (`language`) REFERENCES `language` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2024-01-26 10:47:27
