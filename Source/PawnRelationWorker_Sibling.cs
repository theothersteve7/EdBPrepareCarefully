﻿using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;

namespace EdB.PrepareCarefully
{
	public class PawnRelationWorker_Sibling : PawnRelationWorker
	{
		//
		// Static Methods
		//
		private static Pawn GenerateParent(Pawn generatedChild, Pawn existingChild, Gender genderToGenerate, PawnGenerationRequest childRequest, bool newlyGeneratedParentsWillBeSpousesIfNotGay)
		{
			float ageChronologicalYearsFloat = generatedChild.ageTracker.AgeChronologicalYearsFloat;
			float ageChronologicalYearsFloat2 = existingChild.ageTracker.AgeChronologicalYearsFloat;
			float num = (genderToGenerate != Gender.Male) ? 16 : 14;
			float num2 = (genderToGenerate != Gender.Male) ? 45 : 50;
			float num3 = (genderToGenerate != Gender.Male) ? 27 : 30;
			float num4 = Mathf.Max(ageChronologicalYearsFloat, ageChronologicalYearsFloat2) + num;
			float maxChronologicalAge = num4 + (num2 - num);
			float midChronologicalAge = num4 + (num3 - num);
			float value;
			float value2;
			float value3;
			string last;
			PawnRelationWorker_Sibling.GenerateParentParams(num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, out value, out value2, out value3, out last);
			bool allowGay = true;
			if (newlyGeneratedParentsWillBeSpousesIfNotGay && last.NullOrEmpty() && Rand.Value < 0.8) {
				if (genderToGenerate == Gender.Male && existingChild.GetMother() != null && !existingChild.GetMother().story.traits.HasTrait(TraitDefOf.Gay)) {
					last = ((NameTriple)existingChild.GetMother().Name).Last;
					allowGay = false;
				}
				else if (genderToGenerate == Gender.Female && existingChild.GetFather() != null && !existingChild.GetFather().story.traits.HasTrait(TraitDefOf.Gay)) {
					last = ((NameTriple)existingChild.GetFather().Name).Last;
					allowGay = false;
				}
			}
			Faction faction = existingChild.Faction;
			if (faction == null || faction.IsPlayer) {
				bool tryMedievalOrBetter = faction != null && faction.def.techLevel >= TechLevel.Medieval;
				Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter);
			}
			float? fixedChronologicalAge = new float?(value2);
			Gender? fixedGender = new Gender?(genderToGenerate);
			float? fixedSkinWhiteness = new float?(value3);
			PawnGenerationRequest request = new PawnGenerationRequest(existingChild.kindDef, faction, PawnGenerationContext.NonPlayer, true, false, true, true, false, false, 1, false, allowGay, true, null, new float?(value), fixedChronologicalAge, fixedGender, fixedSkinWhiteness, last);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			if (!Find.WorldPawns.Contains(pawn)) {
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Keep);
			}
			return pawn;
		}

		private static void GenerateParentParams(float minChronologicalAge, float maxChronologicalAge, float midChronologicalAge, float minBioAgeToHaveChildren, Pawn generatedChild, Pawn existingChild, PawnGenerationRequest childRequest, out float biologicalAge, out float chronologicalAge, out float skinWhiteness, out string lastName)
		{
			chronologicalAge = Rand.GaussianAsymmetric(midChronologicalAge, (midChronologicalAge - minChronologicalAge) / 2, (maxChronologicalAge - midChronologicalAge) / 2);
			chronologicalAge = Mathf.Clamp(chronologicalAge, minChronologicalAge, maxChronologicalAge);
			biologicalAge = Rand.Range(minBioAgeToHaveChildren, Mathf.Min(existingChild.RaceProps.lifeExpectancy, chronologicalAge));
			if (existingChild.GetFather() != null) {
				skinWhiteness = ParentRelationUtility.GetRandomSecondParentSkinColor(existingChild.GetFather().story.skinWhiteness, existingChild.story.skinWhiteness, childRequest.FixedSkinWhiteness);
			}
			else if (existingChild.GetMother() != null) {
				skinWhiteness = ParentRelationUtility.GetRandomSecondParentSkinColor(existingChild.GetMother().story.skinWhiteness, existingChild.story.skinWhiteness, childRequest.FixedSkinWhiteness);
			}
			else if (!childRequest.FixedSkinWhiteness.HasValue) {
				skinWhiteness = PawnSkinColors.GetRandomSkinColorSimilarTo(existingChild.story.skinWhiteness, 0, 1);
			}
			else {
				float num = Mathf.Min(childRequest.FixedSkinWhiteness.Value, existingChild.story.skinWhiteness);
				float num2 = Mathf.Max(childRequest.FixedSkinWhiteness.Value, existingChild.story.skinWhiteness);
				if (Rand.Value < 0.5) {
					skinWhiteness = PawnSkinColors.GetRandomSkinColorSimilarTo(num, 0, num);
				}
				else {
					skinWhiteness = PawnSkinColors.GetRandomSkinColorSimilarTo(num2, num2, 1);
				}
			}
			lastName = null;
			if (!ChildRelationUtility.DefinitelyHasNotBirthName(existingChild) && ChildRelationUtility.ChildWantsNameOfAnyParent(existingChild)) {
				if (existingChild.GetMother() == null && existingChild.GetFather() == null) {
					if (Rand.Value < 0.5) {
						lastName = ((NameTriple)existingChild.Name).Last;
					}
				}
				else {
					string last = ((NameTriple)existingChild.Name).Last;
					string b = null;
					if (existingChild.GetMother() != null) {
						b = ((NameTriple)existingChild.GetMother().Name).Last;
					}
					else if (existingChild.GetFather() != null) {
						b = ((NameTriple)existingChild.GetFather().Name).Last;
					}
					if (last != b) {
						lastName = last;
					}
				}
			}
		}

		private static void ResolveMyName(ref PawnGenerationRequest request, Pawn generated)
		{
			if (request.FixedLastName != null) {
				return;
			}
			if (ChildRelationUtility.ChildWantsNameOfAnyParent(generated)) {
				if (Rand.Value < 0.5) {
					request.SetFixedLastName(((NameTriple)generated.GetFather().Name).Last);
				}
				else {
					request.SetFixedLastName(((NameTriple)generated.GetMother().Name).Last);
				}
			}
		}

		private static void ResolveMySkinColor(ref PawnGenerationRequest request, Pawn generated)
		{
			if (request.FixedSkinWhiteness.HasValue) {
				return;
			}
			request.SetFixedSkinWhiteness(ChildRelationUtility.GetRandomChildSkinColor(generated.GetFather().story.skinWhiteness, generated.GetMother().story.skinWhiteness));
		}

		//
		// Methods
		//
		public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
		{
			bool otherPawnHasMother = other.GetMother() != null;
			bool otherPawnHasFather = other.GetFather() != null;
			if (generated.GetMother() != null && generated.GetFather() != null && !otherPawnHasMother && !otherPawnHasFather) {
				other.SetMother(generated.GetMother());
				other.SetFather(generated.GetFather());
			}
			else {
				bool flag = other.GetMother() != null;
				bool flag2 = other.GetFather() != null;
				bool flag3 = Rand.Value < 0.85;
				if (flag && LovePartnerRelationUtility.HasAnyLovePartner(other.GetMother())) {
					flag3 = false;
				}
				if (flag2 && LovePartnerRelationUtility.HasAnyLovePartner(other.GetFather())) {
					flag3 = false;
				}
				if (!flag) {
					Pawn newMother = PawnRelationWorker_Sibling.GenerateParent(generated, other, Gender.Female, request, flag3);
					other.SetMother(newMother);
				}
				generated.SetMother(other.GetMother());
				if (!flag2) {
					Pawn newFather = PawnRelationWorker_Sibling.GenerateParent(generated, other, Gender.Male, request, flag3);
					other.SetFather(newFather);
				}
				generated.SetFather(other.GetFather());
				if (!flag || !flag2) {
					bool flag4 = other.GetMother().story.traits.HasTrait(TraitDefOf.Gay) || other.GetFather().story.traits.HasTrait(TraitDefOf.Gay);
					if (flag4) {
						other.GetFather().relations.AddDirectRelation(PawnRelationDefOf.ExLover, other.GetMother());
					}
					else if (flag3) {
						other.GetFather().relations.AddDirectRelation(PawnRelationDefOf.Spouse, other.GetMother());
					}
					else {
						LovePartnerRelationUtility.GiveRandomExLoverOrExSpouseRelation(other.GetFather(), other.GetMother());
					}
				}
				PawnRelationWorker_Sibling.ResolveMyName(ref request, generated);
				PawnRelationWorker_Sibling.ResolveMySkinColor(ref request, generated);
			}
		}

		public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
		{
			float num = 1;
			float num2 = 1;
			if (other.GetFather() != null || other.GetMother() != null) {
				num = ChildRelationUtility.ChanceOfBecomingChildOf(generated, other.GetFather(), other.GetMother(), new PawnGenerationRequest?(request), null, null);
			}
			else if (request.FixedSkinWhiteness.HasValue) {
				num2 = ChildRelationUtility.GetSkinSimilarityFactor(request.FixedSkinWhiteness.Value, other.story.skinWhiteness);
			}
			else {
				num2 = PawnSkinColors.GetWhitenessCommonalityFactor(other.story.skinWhiteness);
			}
			float num3 = Mathf.Abs(generated.ageTracker.AgeChronologicalYearsFloat - other.ageTracker.AgeChronologicalYearsFloat);
			float num4 = 1;
			if (num3 > 40) {
				num4 = 0.2f;
			}
			else if (num3 > 10) {
				num4 = 0.65f;
			}
			return num * num2 * num4 * base.BaseGenerationChanceFactor(generated, other, request);
		}

		public override bool InRelation(Pawn me, Pawn other)
		{
			return me != other && (me.GetMother() != null && me.GetFather() != null && me.GetMother() == other.GetMother() && me.GetFather() == other.GetFather());
		}
	}
}
