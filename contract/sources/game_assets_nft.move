// Copyright (c) Mysten Labs, Inc.
// SPDX-License-Identifier: Apache-2.0

module examples::game_assets;

use std::string;
use sui::event;
use sui::url::{Self, Url};

/// An example NFT that can be minted by anybody
public struct GameAssetsNFT has key, store {
    id: UID,
    /// Name for the token
    name: string::String,
    /// Description of the token
    description: string::String,
    /// URL for the token
    url: Url,
}

// ===== Events =====

public struct NFTMinted has copy, drop {
    // The Object ID of the NFT
    object_id: ID,
    // The creator of the NFT
    creator: address,
    // The name of the NFT
    name: string::String,
}

// ===== Public view functions =====

/// Get the NFT's `name`
public fun name(nft: &GameAssetsNFT): &string::String {
    &nft.name
}

/// Get the NFT's `description`
public fun description(nft: &GameAssetsNFT): &string::String {
    &nft.description
}

/// Get the NFT's `url`
public fun url(nft: &GameAssetsNFT): &Url {
    &nft.url
}

// ===== Entrypoints =====

#[allow(lint(self_transfer))]
public fun mint_to_sender(
    name: vector<u8>,
    description: vector<u8>,
    url: vector<u8>,
    ctx: &mut TxContext,
) {
    let sender = ctx.sender();
    let nft = GameAssetsNFT {
        id: object::new(ctx),
        name: string::utf8(name),
        description: string::utf8(description),
        url: url::new_unsafe_from_bytes(url),
    };

    event::emit(NFTMinted {
        object_id: object::id(&nft),
        creator: sender,
        name: nft.name,
    });

    transfer::public_transfer(nft, sender);
}

/// Transfer `nft` to `recipient`
public fun transfer(nft: GameAssetsNFT, recipient: address, _: &mut TxContext) {
    transfer::public_transfer(nft, recipient)
}

/// Update the `description` of `nft` to `new_description`
public fun update_description(
    nft: &mut GameAssetsNFT,
    new_description: vector<u8>,
    _: &mut TxContext,
) {
    nft.description = string::utf8(new_description)
}

/// Permanently delete `nft`
public fun burn(nft: GameAssetsNFT, _: &mut TxContext) {
    let GameAssetsNFT { id, name: _, description: _, url: _ } = nft;
    id.delete()
}
